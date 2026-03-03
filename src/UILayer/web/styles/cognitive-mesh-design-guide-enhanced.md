
# 🧠 Cognitive Mesh Visual Design Guide – Enhanced Edition
**Neural Space Opera Design System**

*Theme: Neo-cybernetic command HUD for modular AI ecosystems*

---

## 🔁 Design System Architecture

This design system uses a unified token model across:

- `CSS Custom Properties` for runtime styling
- `SCSS Variables` for authoring and mixins
- `JSON Tokens` for Figma/Tailwind integration
- `Visual & Behavioral Patterns` for animations, interactions, AI-state overlays

---

## 🎨 Color System (Extended with Harmonies)

### Primary Neural Palette
| Token | Color | RGB | Usage |
|-------|-------|-----|--------|
| `--neural-primary` | #58A6FF | `rgb(88, 166, 255)` | Main brand, neural threads |
| `--neural-secondary` | #8B45FF | `rgb(139, 69, 255)` | Secondary brand, outlines |
| `--neural-accent` | #00BFFF | `rgb(0, 191, 255)` | Highlights, active nodes |
| `--neural-matrix` | #7EE787 | `rgb(126, 231, 135)` | Success/valid states |

### Harmonies
| Token | Light (100) | Base (500) | Dark (900) |
|-------|-------------|------------|------------|
| `--color-primary` | `#CDE7FF` | `#58A6FF` | `#1B2940` |
| `--color-secondary` | `#E2CCFF` | `#8B45FF` | `#2E1A47` |
| `--color-matrix` | `#CFF5D7` | `#7EE787` | `#183E29` |

---

## 🔤 Typography System

### Fonts
```css
--font-mono: 'JetBrains Mono', monospace;
--font-sans: 'Inter', sans-serif;
```

### Type Scale
| Name | Size | Weight | Line-height | Use |
|------|------|--------|-------------|-----|
| Hero | 64px | 700 | 1.2 | Page headers |
| Title | 30px | 600 | 1.4 | Section headers |
| Label | 16px | 600 | 1.4 | UI elements |
| Body | 16px | 400 | 1.6 | Default text |
| Code | 14px | 400 | 1.6 | Terminal text |
| Caption | 12px | 400 | 1.4 | Metadata |

---

## 🌌 Glow Effects

| Name | CSS | Use |
|------|-----|-----|
| Primary | `0 0 20px rgba(88,166,255,1)` | Buttons, borders |
| Soft | `0 0 40px rgba(88,166,255,0.3)` | Panel ambient |
| Intense | `0 0 60px rgba(88,166,255,1.5)` | Focus, alerts |
| Multi | layered shadows | Hero components |

---

## 🧠 AI-State Overlays

| State | Color | Visual |
|-------|-------|--------|
| Thinking | Blue pulse | Animated border |
| Hallucinating | Violet shimmer | Subtle glitch FX |
| Verified | Matrix green | Pulse glow |

---

## ✨ Animation Guide

| Name | Duration | Curve | Purpose |
|------|----------|-------|---------|
| Fast | 150ms | ease-out | Hovers |
| Normal | 300ms | `ease` | Tabs, clicks |
| Neural | 600ms | `cubic-bezier(0.4,0,0.2,1)` | Main UI |
| Background | 10s+ | linear | Particles |

---

## 🧩 Component States

### Buttons
```css
.btn-neural:disabled {
  background: rgba(88,166,255,0.1);
  color: rgba(255,255,255,0.4);
  cursor: not-allowed;
}
```

### Metric Card (Loading)
```css
.metric-card.loading::after {
  content: 'Loading...';
  opacity: 0.6;
  font-size: 12px;
}
```

---

## 📦 Output Formats

| Tool | Output |
|------|--------|
| SCSS | Variables + Mixins |
| CSS | Custom Properties |
| Figma | Tokens via JSON |
| Tailwind | Theme config |

---

## 🧭 Grid & Layout System

### Spacing
```css
--spacing-sm: 8px;
--spacing-md: 16px;
--spacing-lg: 32px;
```

### Border Radius
```css
--radius-sm: 4px;
--radius-lg: 16px;
--radius-full: 9999px;
```

### Z-Index Layers
| Layer | Z |
|-------|---|
| Particles | 0 |
| Panels | 10 |
| Tooltips | 100 |
| Modals | 200 |

---

## 🧬 Final Notes

- **Toolchain-Agnostic**: Tokens can be used in raw CSS, SASS, Tailwind, or Figma
- **Performance-Conscious**: Glow animations capped at 5 concurrent FX
- **Accessible**: All UI meets or exceeds 4.5:1 contrast, with reduced-motion support

---

