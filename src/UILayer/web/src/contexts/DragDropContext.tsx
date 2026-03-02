"use client"
import React, { createContext, useCallback, useContext, useEffect, useRef, useState } from "react"

export interface DragItem {
  id: string
  type: string
  size: "small" | "medium" | "large" | "x-large"
  position: { x: number; y: number }
  isDocked: boolean
  dockedZone?: string
  zIndex: number
  originalPosition?: { x: number; y: number }
}

export interface DockZone {
  id: string
  label: string
  bounds: { x: number; y: number; width: number; height: number }
  maxItems?: number
  allowedSizes?: ("small" | "medium" | "large" | "x-large")[]
  items: string[]
  isResizable?: boolean
  minWidth?: number
  minHeight?: number
}

interface DragDropContextType {
  draggedItem: DragItem | null
  items: { [key: string]: DragItem }
  dockZones: { [key: string]: DockZone }
  isDragging: boolean
  activeDockZone: string | null
  globalSize: "small" | "medium" | "large" | "x-large"
  snapToGrid: boolean
  showGrid: boolean
  startDrag: (item: DragItem, event: React.MouseEvent) => void
  endDrag: () => void
  updateItemSize: (id: string, size: "small" | "medium" | "large" | "x-large") => void
  setGlobalSize: (size: "small" | "medium" | "large" | "x-large") => void
  dockItem: (itemId: string, zoneId: string, position?: number) => boolean
  undockItem: (itemId: string) => void
  registerDockZone: (zone: Omit<DockZone, "bounds">) => void
  updateDockZoneBounds: (zoneId: string, bounds: { x: number; y: number; width: number; height: number }) => void
  resizeDockZone: (zoneId: string, bounds: { x: number; y: number; width: number; height: number }) => void
  unregisterDockZone: (zoneId: string) => void
  bringToFront: (itemId: string) => void
  registerItem: (item: DragItem) => void
  swapItems: (item1Id: string, item2Id: string) => void
  toggleSnapToGrid: () => void
  toggleShowGrid: () => void
  getDockedItemsForZone: (zoneId: string) => DragItem[]
  dockZonePositions: { [key: string]: { x: number, y: number } }
  dockZoneStack: string[]
  updateDockZonePosition: (zoneId: string, x: number, y: number) => void
  getDockZoneStack: () => string[]
  insertDockZoneIntoStack: (zoneId: string, index: number) => void
}

const DragDropContext = createContext<DragDropContextType | null>(null)

export const useDragDrop = () => {
  const context = useContext(DragDropContext)
  if (!context) {
    throw new Error("useDragDrop must be used within a DragDropProvider")
  }
  return context
}

const GRID_SIZE = 20

export const DragDropProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [draggedItem, setDraggedItem] = useState<DragItem | null>(null)
  const [items, setItems] = useState<{ [key: string]: DragItem }>({})
  const [dockZones, setDockZones] = useState<{ [key: string]: DockZone }>({})
  const [isDragging, setIsDragging] = useState(false)
  const [activeDockZone, setActiveDockZone] = useState<string | null>(null)
  const [globalSize, setGlobalSize] = useState<"small" | "medium" | "large" | "x-large">("medium")
  const [snapToGrid, setSnapToGrid] = useState(true)
  const [showGrid, setShowGrid] = useState(false)
  const [dockZonePositions, setDockZonePositions] = useState<{ [key: string]: { x: number, y: number } }>({})
  const [dockZoneStack, setDockZoneStack] = useState<string[]>([])

  const dockZonesRef = useRef<{ [key: string]: DockZone }>({})
  const itemsRef = useRef<{ [key: string]: DragItem }>({})
  const dragOffset = useRef({ x: 0, y: 0 })
  const maxZIndex = useRef(100)

  // Update refs when state changes
  useEffect(() => {
    dockZonesRef.current = dockZones
    itemsRef.current = items
  }, [dockZones, items])

  // Apply global size changes to all items
  useEffect(() => {
    setItems((prev) => {
      const updated = { ...prev }
      Object.keys(updated).forEach((itemId) => {
        if (!updated[itemId].isDocked) {
          updated[itemId] = {
            ...updated[itemId],
            size: globalSize,
          }
        }
      })
      return updated
    })
  }, [globalSize])

  const snapToGridPosition = useCallback(
    (position: { x: number; y: number }) => {
      if (!snapToGrid) return position
      return {
        x: Math.round(position.x / GRID_SIZE) * GRID_SIZE,
        y: Math.round(position.y / GRID_SIZE) * GRID_SIZE,
      }
    },
    [snapToGrid],
  )

  const checkDockZoneCollision = useCallback((position: { x: number; y: number }, item: DragItem) => {
    let foundZone: string | null = null
    let bestZone: string | null = null
    let maxOverlap = 0
    let minCenterDist = Infinity
    const zones = dockZonesRef.current

    Object.entries(zones).forEach(([zoneId, zone]) => {
      const itemBounds = getSizeConfig(item.size)

      // Calculate overlap area for better collision detection
      const overlapLeft = Math.max(position.x, zone.bounds.x)
      const overlapTop = Math.max(position.y, zone.bounds.y)
      const overlapRight = Math.min(position.x + itemBounds.width, zone.bounds.x + zone.bounds.width)
      const overlapBottom = Math.min(position.y + itemBounds.height, zone.bounds.y + zone.bounds.height)

      const overlapWidth = Math.max(0, overlapRight - overlapLeft)
      const overlapHeight = Math.max(0, overlapBottom - overlapTop)
      const overlapArea = overlapWidth * overlapHeight

      // Check if there's significant overlap (at least 30% of the item)
      const itemArea = itemBounds.width * itemBounds.height
      const overlapPercentage = overlapArea / itemArea

      // Calculate center-to-center distance
      const itemCenter = { x: position.x + itemBounds.width / 2, y: position.y + itemBounds.height / 2 }
      const zoneCenter = { x: zone.bounds.x + zone.bounds.width / 2, y: zone.bounds.y + zone.bounds.height / 2 }
      const centerDist = Math.sqrt(
        Math.pow(itemCenter.x - zoneCenter.x, 2) + Math.pow(itemCenter.y - zoneCenter.y, 2)
      )

      if (
        overlapPercentage > 0.3 && // At least 30% overlap
        (!zone.allowedSizes || zone.allowedSizes.includes(item.size)) &&
        (!zone.maxItems || zone.items.length < zone.maxItems)
      ) {
        if (overlapArea > maxOverlap) {
          maxOverlap = overlapArea
          bestZone = zoneId
          minCenterDist = centerDist
        } else if (overlapArea === maxOverlap && centerDist < minCenterDist) {
          // Tiebreaker: closest by center
          bestZone = zoneId
          minCenterDist = centerDist
        }
        foundZone = zoneId
      }
    })

    // Use the zone with the most overlap, tiebreak by center distance
    const activeZone = bestZone || foundZone
    setActiveDockZone(activeZone)
    return activeZone
  }, [])

  const registerItem = useCallback((item: DragItem) => {
    setItems((prev) => {
      if (prev[item.id]) return prev
      return {
        ...prev,
        [item.id]: item,
      }
    })
  }, [])

  const startDrag = useCallback(
    (item: DragItem, event: React.MouseEvent) => {
      const rect = (event.currentTarget as HTMLElement).getBoundingClientRect()
      dragOffset.current = {
        x: event.clientX - rect.left,
        y: event.clientY - rect.top,
      }

      // If item is docked, undock it first and position it at mouse
      if (item.isDocked) {
        const newPosition = {
          x: event.clientX - dragOffset.current.x,
          y: event.clientY - dragOffset.current.y,
        }

        // Undock the item
        setDockZones((prevZones) => {
          const updatedZones = { ...prevZones }
          if (item.dockedZone && updatedZones[item.dockedZone]) {
            updatedZones[item.dockedZone] = {
              ...updatedZones[item.dockedZone],
              items: updatedZones[item.dockedZone].items.filter((id) => id !== item.id),
            }
          }
          return updatedZones
        })

        // Update item state
        setItems((prev) => ({
          ...prev,
          [item.id]: {
            ...prev[item.id],
            position: newPosition,
            isDocked: false,
            dockedZone: undefined,
            size: globalSize, // Apply global size when undocking
          },
        }))

        // Update the item reference for dragging
        item = {
          ...item,
          position: newPosition,
          isDocked: false,
          dockedZone: undefined,
          size: globalSize,
        }
      }

      const itemWithOriginal = {
        ...item,
        originalPosition: { ...item.position },
      }

      setDraggedItem(itemWithOriginal)
      setIsDragging(true)

      // Bring to front
      maxZIndex.current += 1
      setItems((prev) => ({
        ...prev,
        [item.id]: {
          ...prev[item.id],
          zIndex: maxZIndex.current,
        },
      }))

      const handleMouseMove = (e: MouseEvent) => {
        const newPosition = snapToGridPosition({
          x: e.clientX - dragOffset.current.x,
          y: e.clientY - dragOffset.current.y,
        })

        // Update position immediately for smooth dragging
        setItems((prev) => ({
          ...prev,
          [item.id]: {
            ...prev[item.id],
            position: newPosition,
            isDocked: false, // Ensure item is not docked while dragging
          },
        }))

        // Check dock zone collision
        checkDockZoneCollision(newPosition, item)
      }

      const handleMouseUp = () => {
        const currentActiveDockZone = activeDockZone

        // Auto-dock if dropped in a valid zone
        if (currentActiveDockZone && draggedItem) {
          dockItem(draggedItem.id, currentActiveDockZone)
        }

        // Clean up drag state
        setDraggedItem(null)
        setIsDragging(false)
        setActiveDockZone(null)
        document.removeEventListener("mousemove", handleMouseMove)
        document.removeEventListener("mouseup", handleMouseUp)
      }

      document.addEventListener("mousemove", handleMouseMove)
      document.addEventListener("mouseup", handleMouseUp)
    },
    [checkDockZoneCollision, activeDockZone, draggedItem, snapToGridPosition, globalSize],
  )

  const endDrag = useCallback(() => {
    setDraggedItem(null)
    setIsDragging(false)
    setActiveDockZone(null)
  }, [])

  const updateItemSize = useCallback((id: string, size: "small" | "medium" | "large" | "x-large") => {
    setItems((prev) => ({
      ...prev,
      [id]: {
        ...prev[id],
        size,
      },
    }))
  }, [])

  const swapItems = useCallback((item1Id: string, item2Id: string) => {
    setItems((prev) => {
      const item1 = prev[item1Id]
      const item2 = prev[item2Id]

      if (!item1 || !item2) return prev

      return {
        ...prev,
        [item1Id]: {
          ...item1,
          position: item2.position,
        },
        [item2Id]: {
          ...item2,
          position: item1.position,
        },
      }
    })
  }, [])

  const dockItem = useCallback(
    (itemId: string, zoneId: string, position?: number): boolean => {
      console.log(`Attempting to dock item ${itemId} to zone ${zoneId}`)

      const zone = dockZones[zoneId]
      if (!zone) {
        console.warn(`Dock zone ${zoneId} not found`)
        return false
      }

      const item = items[itemId]
      if (!item) {
        console.warn(`Item ${itemId} not found`)
        return false
      }

      // Check if zone can accept the item
      if (zone.maxItems && zone.items.length >= zone.maxItems) {
        console.warn(`Dock zone ${zoneId} is full`)
        return false
      }

      if (zone.allowedSizes && !zone.allowedSizes.includes(item.size)) {
        console.warn(`Item size ${item.size} not allowed in zone ${zoneId}`)
        return false
      }

      // Update item state
      setItems((prev) => ({
        ...prev,
        [itemId]: {
          ...prev[itemId],
          isDocked: true,
          dockedZone: zoneId,
        },
      }))

      // Update zone state
      setDockZones((prev) => {
        const currentZone = prev[zoneId]
        if (!currentZone) return prev

        const newItems = [...currentZone.items.filter((id) => id !== itemId)]
        if (position !== undefined && position >= 0 && position <= newItems.length) {
          newItems.splice(position, 0, itemId)
        } else {
          newItems.push(itemId)
        }

        return {
          ...prev,
          [zoneId]: {
            ...currentZone,
            items: newItems,
          },
        }
      })

      console.log(`Successfully docked item ${itemId} to zone ${zoneId}`)
      return true
    },
    [dockZones, items, setItems, setDockZones],
  )

  const undockItem = useCallback(
    (itemId: string) => {
      setItems((prev) => {
        const item = prev[itemId]
        if (item?.dockedZone) {
          setDockZones((prevZones) => ({
            ...prevZones,
            [item.dockedZone!]: {
              ...prevZones[item.dockedZone!],
              items: prevZones[item.dockedZone!].items.filter((id) => id !== itemId),
            },
          }))

          // Position the item at a visible location when undocked
          const newPosition = item.originalPosition || {
            x: Math.max(50, Math.min(window.innerWidth - 400, 200)),
            y: Math.max(50, Math.min(window.innerHeight - 300, 200)),
          }

          return {
            ...prev,
            [itemId]: {
              ...prev[itemId],
              isDocked: false,
              dockedZone: undefined,
              position: newPosition,
              size: globalSize, // Apply global size when undocking
            },
          }
        }
        return prev
      })
    },
    [globalSize],
  )

  const registerDockZone = useCallback((zone: Omit<DockZone, "bounds">) => {
    console.log(`Registering dock zone: ${zone.id}`)
    setDockZones((prev) => {
      if (prev[zone.id]) return prev
      return {
        ...prev,
        [zone.id]: {
          ...zone,
          bounds: { x: 0, y: 0, width: 0, height: 0 },
          isResizable: zone.isResizable ?? true,
          minWidth: zone.minWidth ?? 200,
          minHeight: zone.minHeight ?? 150,
        },
      }
    })
  }, [])

  const updateDockZoneBounds = useCallback(
    (zoneId: string, bounds: { x: number; y: number; width: number; height: number }) => {
      setDockZones((prev) => {
        const currentZone = prev[zoneId]
        if (!currentZone) return prev

        const currentBounds = currentZone.bounds
        if (
          currentBounds.x === bounds.x &&
          currentBounds.y === bounds.y &&
          currentBounds.width === bounds.width &&
          currentBounds.height === bounds.height
        ) {
          return prev
        }

        return {
          ...prev,
          [zoneId]: {
            ...currentZone,
            bounds,
          },
        }
      })
    },
    [],
  )

  const resizeDockZone = useCallback(
    (zoneId: string, bounds: { x: number; y: number; width: number; height: number }) => {
      setDockZones((prev) => {
        const zone = prev[zoneId]
        if (!zone || !zone.isResizable) return prev

        const minWidth = zone.minWidth || 200
        const minHeight = zone.minHeight || 150

        const constrainedBounds = {
          ...bounds,
          width: Math.max(bounds.width, minWidth),
          height: Math.max(bounds.height, minHeight),
        }

        return {
          ...prev,
          [zoneId]: {
            ...zone,
            bounds: constrainedBounds,
          },
        }
      })
    },
    [],
  )

  const unregisterDockZone = useCallback((zoneId: string) => {
    setDockZones((prev) => {
      const newZones = { ...prev }
      delete newZones[zoneId]
      return newZones
    })
  }, [])

  const bringToFront = useCallback((itemId: string) => {
    maxZIndex.current += 1
    setItems((prev) => ({
      ...prev,
      [itemId]: {
        ...prev[itemId],
        zIndex: maxZIndex.current,
      },
    }))
  }, [])

  const toggleSnapToGrid = useCallback(() => {
    setSnapToGrid((prev) => !prev)
  }, [])

  const toggleShowGrid = useCallback(() => {
    setShowGrid((prev) => !prev)
  }, [])

  const getDockedItemsForZone = useCallback(
    (zoneId: string) => {
      const zone = dockZones[zoneId]
      if (!zone) return []

      // Filter out invalid items and ensure they exist
      return zone.items.map((itemId) => items[itemId]).filter((item) => item && item.id && typeof item.id === "string")
    },
    [dockZones, items],
  )

  // Update a DockZone's position
  const updateDockZonePosition = useCallback((zoneId: string, x: number, y: number) => {
    setDockZonePositions((prev) => ({ ...prev, [zoneId]: { x, y } }))
  }, [])

  // Get current stack order
  const getDockZoneStack = useCallback(() => dockZoneStack, [dockZoneStack])

  // Insert a DockZone into the stack at a given index
  const insertDockZoneIntoStack = useCallback((zoneId: string, index: number) => {
    setDockZoneStack((prev) => {
      const filtered = prev.filter((id) => id !== zoneId)
      filtered.splice(index, 0, zoneId)
      return filtered
    })
  }, [])

  const value: DragDropContextType = {
    draggedItem,
    items,
    dockZones,
    isDragging,
    activeDockZone,
    globalSize,
    snapToGrid,
    showGrid,
    startDrag,
    endDrag,
    updateItemSize,
    setGlobalSize,
    dockItem,
    undockItem,
    registerDockZone,
    updateDockZoneBounds,
    resizeDockZone,
    unregisterDockZone,
    bringToFront,
    registerItem,
    swapItems,
    toggleSnapToGrid,
    toggleShowGrid,
    getDockedItemsForZone,
    dockZonePositions,
    dockZoneStack,
    updateDockZonePosition,
    getDockZoneStack,
    insertDockZoneIntoStack,
  }

  return <DragDropContext.Provider value={value}>{children}</DragDropContext.Provider>
}

export const getSizeConfig = (size: "small" | "medium" | "large" | "x-large") => {
  switch (size) {
    case "small":
      return { width: 280, height: 200, className: "w-70 h-50" }
    case "medium":
      return { width: 400, height: 300, className: "w-100 h-75" }
    case "large":
      return { width: 600, height: 450, className: "w-150 h-112" }
    case "x-large":
      return { width: 800, height: 600, className: "w-200 h-150" }
    default:
      return { width: 400, height: 300, className: "w-100 h-75" }
  }
}
