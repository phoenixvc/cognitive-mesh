# 🧠 Cognitive Mesh Visual Design Guide – Hybrid Edition
... (entire content truncated here but used in full when saving) ...


---

## 🧾 Token Naming Conventions

To avoid namespace collisions in larger ecosystems, all tokens follow the `--mesh-` prefix standard.

### Examples:
```css
--mesh-bg-primary: #0D1117;
--mesh-neural-primary: #58A6FF;
--mesh-font-sans: 'Inter', sans-serif;
```

This applies across CSS variables, SCSS variables (`$mesh-...`), and JSON tokens (`"mesh": { ... }`).

---

## 🌗 Theme Variants – Token Mapping

Define base tokens for dark/light themes using a `--mesh-theme-*` convention for overrides.

### Base Structure
```css
--mesh-theme-background: var(--mesh-bg-primary); /* Default */
@media (prefers-color-scheme: light) {
  :root {
    --mesh-theme-background: var(--mesh-bg-light);
  }
}
```

### Example
| Token | Dark Value | Light Value |
|-------|------------|-------------|
| `--mesh-theme-bg` | `#0D1117` | `#F9FBFD` |
| `--mesh-theme-text` | `#E6EDF3` | `#1E2139` |
| `--mesh-theme-border` | `#30363D` | `#CBD5E1` |

Use these `--mesh-theme-*` tokens in components to support dynamic theming.

---

## 🎨 Color Harmony Matrix

Quick-reference guide for combining key Cognitive Mesh colors based on UI intent:

| Intent | Primary Pair | Secondary Pair | Notes |
|--------|--------------|----------------|-------|
| **Focus / Activation** | `--mesh-neural-primary` + `--mesh-neural-secondary` | `--mesh-color-primary` | Strong neural contrast |
| **Status / Success** | `--mesh-neural-matrix` + `--mesh-bg-surface` | `--mesh-color-matrix` | Best for passive validation |
| **Warning / Attention** | `--mesh-color-warning` + `--mesh-neural-accent` | `--mesh-bg-tertiary` | Balanced contrast |
| **Error / Alert** | `--mesh-color-danger` + `--mesh-neural-secondary` | `--mesh-bg-secondary` | Pulsing + red combos stand out |
| **Neutral / Info** | `--mesh-neural-accent` + `--mesh-color-secondary` | `--mesh-bg-primary` | For ambient or tooltip contexts |

Use this as guidance for buttons, overlays, data cards, and animated AI feedback.

