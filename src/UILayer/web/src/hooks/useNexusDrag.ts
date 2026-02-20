import { useState } from "react";

export function useNexusDrag({ onDragStart, onDragEnd }: { onDragStart?: () => void; onDragEnd?: () => void }) {
  const [isDragging, setIsDragging] = useState(false);
  const start = () => {
    setIsDragging(true);
    onDragStart?.();
  };
  const end = () => {
    setIsDragging(false);
    onDragEnd?.();
  };
  return { isDragging, start, end };
} 