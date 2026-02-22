"use client";
import { useEffect, useRef } from "react";

interface Particle {
  x: number;
  y: number;
  xv: number;
  yv: number;
  color: string;
  radius: number;
}

interface Explosion {
  x: number;
  y: number;
  start: number;
}

const PARTICLE_COUNT = 12;
const PARTICLE_RADIUS = 3;
const PARTICLE_COLOR = "rgba(6,182,212,0.7)";
const PARTICLE_GLOW = "rgba(6,182,212,0.3)";
const EXPLOSION_RADIUS = 24;
const EXPLOSION_DURATION = 400; // ms
const COLLISION_DISTANCE = 40;

function randomVelocity() {
  const angle = Math.random() * 2 * Math.PI;
  const speed = 0.15 + Math.random() * 0.12;
  return { x: Math.cos(angle) * speed, y: Math.sin(angle) * speed };
}

function randomPosition(width: number, height: number) {
  return {
    x: Math.random() * width,
    y: Math.random() * height,
  };
}

function spawnParticle(width: number, height: number): Particle {
  const { x, y } = randomPosition(width, height);
  const { x: xv, y: yv } = randomVelocity();
  return {
    x,
    y,
    xv,
    yv,
    color: PARTICLE_COLOR,
    radius: PARTICLE_RADIUS,
  };
}

export default function ParticleField() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const particles = useRef<Particle[]>([]);
  const explosions = useRef<Explosion[]>([]);
  const animationRef = useRef<number | undefined>(undefined);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    let width = window.innerWidth;
    let height = window.innerHeight;
    canvas.width = width;
    canvas.height = height;

    function spawnPair() {
      particles.current.push(spawnParticle(width, height), spawnParticle(width, height));
    }

    // Initialize particles
    particles.current = [];
    for (let i = 0; i < PARTICLE_COUNT; i++) {
      particles.current.push(spawnParticle(width, height));
    }

    function animate() {
      if (!ctx) return;
      ctx.clearRect(0, 0, width, height);
      // Draw and update particles
      for (let i = 0; i < particles.current.length; i++) {
        const p = particles.current[i];
        // Move
        p.x += p.xv;
        p.y += p.yv;
        // Bounce off edges
        if (p.x < 0 || p.x > width) p.xv *= -1;
        if (p.y < 0 || p.y > height) p.yv *= -1;
        // Draw glow
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.radius * 3, 0, 2 * Math.PI);
        ctx.fillStyle = PARTICLE_GLOW;
        ctx.fill();
        // Draw core
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.radius, 0, 2 * Math.PI);
        ctx.fillStyle = p.color;
        ctx.shadowColor = p.color;
        ctx.shadowBlur = 8;
        ctx.fill();
        ctx.shadowBlur = 0;
      }
      // Check for collisions (rare)
      const toRemove: number[] = [];
      for (let i = 0; i < particles.current.length; i++) {
        for (let j = i + 1; j < particles.current.length; j++) {
          const a = particles.current[i];
          const b = particles.current[j];
          const dx = a.x - b.x;
          const dy = a.y - b.y;
          const dist = Math.sqrt(dx * dx + dy * dy);
          if (dist < COLLISION_DISTANCE) {
            // Trigger explosion at midpoint
            explosions.current.push({
              x: (a.x + b.x) / 2,
              y: (a.y + b.y) / 2,
              start: performance.now(),
            });
            toRemove.push(i, j);
            break;
          }
        }
        if (toRemove.length) break;
      }
      // Remove collided particles and spawn new pair
      if (toRemove.length) {
        particles.current = particles.current.filter((_, idx) => !toRemove.includes(idx));
        spawnPair();
      }
      // Draw and update explosions
      const now = performance.now();
      explosions.current = explosions.current.filter((exp) => {
        const age = now - exp.start;
        if (age > EXPLOSION_DURATION) return false;
        const progress = age / EXPLOSION_DURATION;
        ctx.beginPath();
        ctx.arc(exp.x, exp.y, EXPLOSION_RADIUS * progress, 0, 2 * Math.PI);
        ctx.strokeStyle = `rgba(6,182,212,${0.5 * (1 - progress)})`;
        ctx.lineWidth = 2 + 6 * (1 - progress);
        ctx.stroke();
        return true;
      });
      animationRef.current = requestAnimationFrame(animate);
    }

    animate();
    window.addEventListener("resize", () => {
      width = window.innerWidth;
      height = window.innerHeight;
      canvas.width = width;
      canvas.height = height;
    });
    return () => {
      cancelAnimationFrame(animationRef.current!);
    };
  }, []);

  return (
    <canvas
      ref={canvasRef}
      style={{
        position: "fixed",
        inset: 0,
        zIndex: 0,
        pointerEvents: "none",
        width: "100vw",
        height: "100vh",
        opacity: 0.7,
      }}
      width={window.innerWidth}
      height={window.innerHeight}
      aria-hidden="true"
    />
  );
} 