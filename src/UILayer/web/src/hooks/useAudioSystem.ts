// src/hooks/useAudioSystem.ts

"use client"
import type { AudioConfig, AudioState } from "@/types/nexus"
import { useCallback, useEffect, useRef, useState } from "react"

// Combines pre-loaded audio files and Web Audio API synthesis
export function useAudioSystem(configOrVolume: AudioConfig | number) {
  // Determine if config object or simple initialVolume
  const isConfig = typeof configOrVolume === "object"
  const config: AudioConfig = isConfig
    ? (configOrVolume as AudioConfig)
    : {
        enabled: true,
        volume: configOrVolume as number,
        sounds: { dock: "", undock: "", click: "", snap: "" }
      }

  // HTMLAudioElement map (for URLs)
  const [audioState, setAudioState] = useState<AudioState>({
    isEnabled: config.enabled,
    volume: config.volume,
    sounds: Object.keys(config.sounds).reduce((acc, key) => {
      acc[key as keyof typeof config.sounds] = null
      return acc
    }, {} as Record<keyof typeof config.sounds, HTMLAudioElement | null>)
  })

  // Web Audio API context & buffers
  const audioContextRef = useRef<AudioContext | null>(null)
  const soundBuffersRef = useRef<Map<string, AudioBuffer>>(new Map())

  // Setup both HTMLAudioElements and WebAudio buffers
  useEffect(() => {
    // HTMLAudioElements
    if (isConfig) {
      const loaded = { ...audioState.sounds }
      Object.entries(config.sounds).forEach(([key, src]) => {
        if (src) {
          const audio = new Audio(src)
          audio.volume = config.volume
          loaded[key as keyof typeof config.sounds] = audio
        }
      })
      setAudioState((prev) => ({ ...prev, sounds: loaded }))
    }

    // Web Audio API init
    if (typeof window !== "undefined") {
      audioContextRef.current = new (window.AudioContext || (window as any).webkitAudioContext)()
      const createBuffer = (freq: number, dur: number) => {
        const ctx = audioContextRef.current!
        const sr = ctx.sampleRate
        const buf = ctx.createBuffer(1, sr * dur, sr)
        const data = buf.getChannelData(0)
        for (let i = 0; i < data.length; i++) {
          const t = i / sr
          const env = Math.exp(-t * 10)
          data[i] = Math.sin(2 * Math.PI * freq * t) * env * 0.3
        }
        return buf
      }
      // map default synthesized tones
      soundBuffersRef.current.set("click", createBuffer(1000, 0.08))
      soundBuffersRef.current.set("snap", createBuffer(1200, 0.12))
      soundBuffersRef.current.set("dock", createBuffer(800, 0.15))
      soundBuffersRef.current.set("undock", createBuffer(600, 0.15))
    }
    return () => {
      audioContextRef.current?.close()
    }
  // eslint-disable-next-line
  }, [])

  // Play function covers both types
  const playSound = useCallback(
    (type: string) => {
      if (!audioState.isEnabled) return

      // first try HTMLAudioElement
      const elt = audioState.sounds[type]
      if (elt) {
        elt.currentTime = 0
        elt.volume = audioState.volume
        elt.play().catch(() => {})
        return
      }

      // fallback to Web Audio API
      const ctx = audioContextRef.current
      const buffer = soundBuffersRef.current.get(type)
      if (ctx && buffer) {
        const src = ctx.createBufferSource()
        const gain = ctx.createGain()
        src.buffer = buffer
        gain.gain.value = audioState.volume
        src.connect(gain)
        gain.connect(ctx.destination)
        src.start()
      }
    },
    [audioState]
  )

  const setVolume = useCallback((vol: number) => {
    const v = Math.max(0, Math.min(1, vol))
    setAudioState((prev) => ({ ...prev, volume: v }))
    Object.values(audioState.sounds).forEach((a) => {
      if (a && typeof a.volume === 'number') {
        a.volume = v
      }
    })
  }, [audioState.sounds])

  const toggleAudio = useCallback(() => {
    setAudioState((prev) => ({ ...prev, isEnabled: !prev.isEnabled }))
  }, [])

  const resumeAudio = useCallback(async () => {
    const ctx = audioContextRef.current
    if (ctx && ctx.state === "suspended") await ctx.resume()
  }, [])

  // resume on first user interaction
  useEffect(() => {
    const resume = () => resumeAudio()
    document.addEventListener("click", resume)
    document.addEventListener("keydown", resume)
    return () => {
      document.removeEventListener("click", resume)
      document.removeEventListener("keydown", resume)
    }
  }, [resumeAudio])

  return { audioState, playSound, setVolume, toggleAudio, resumeAudio }
}
