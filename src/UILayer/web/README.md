# Cognitive Mesh UI

A sophisticated, sci-fi inspired user interface built with Next.js, featuring advanced drag-and-drop functionality, real-time energy flow visualization, and a comprehensive design system.

## Features

- **Advanced Drag & Drop System**: Multi-handle drag styles with magnetic snap and gridlines
- **Real-time Energy Flow**: Dynamic particle system with configurable effects
- **Command Nexus**: AI-powered command interface with voice recognition
- **Design System**: Comprehensive token-based design system with Style Dictionary
- **Component Documentation**: Live Storybook documentation for all components
- **Responsive Layout**: Adaptive interface that works across all devices

## Design System

This project includes a comprehensive design system built with **Style Dictionary** that generates design tokens for multiple platforms:

### Design Tokens
- **Colors**: Neural, synaptic, and cortical color palettes with semantic variants
- **Typography**: Inter, JetBrains Mono, and Orbitron font families with complete scale
- **Spacing**: Consistent spacing scale from xs to 6xl
- **Effects**: Gradients, shadows, and animations

### Generated Outputs
- CSS Custom Properties (`build/css/_variables.css`)
- SCSS Variables (`build/scss/_cognitive-mesh-tokens.scss`)
- iOS/Android native formats
- Swift/Java code generation

## Getting Started

### Prerequisites
- Node.js 18+ 
- npm or yarn

### Installation
```bash
npm install
```

### Development
```bash
# Start the main application
npm run dev

# Start Storybook (component documentation)
npm run storybook

# Build design tokens
npm run tokens
```

### Available Scripts
- `npm run dev` - Start Next.js development server
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run storybook` - Start Storybook documentation
- `npm run build-storybook` - Build static Storybook
- `npm run tokens` - Build design tokens with Style Dictionary

## Project Structure

```
CognitiveMeshUI/
├── src/
│   ├── app/                    # Next.js app directory
│   ├── components/             # React components
│   │   ├── CognitiveMeshButton/ # Design system component example
│   │   ├── Nexus/              # Unified nexus component
│   │   ├── DockZone/           # Dock area management
│   │   └── ...                 # Other components
│   ├── contexts/               # React contexts
│   └── hooks/                  # Custom hooks
├── tokens/                     # Style Dictionary token files
│   ├── colors.json            # Color tokens
│   ├── typography.json        # Typography tokens
│   └── spacing.json           # Spacing tokens
├── build/                      # Generated design tokens
│   ├── css/                   # CSS custom properties
│   └── scss/                  # SCSS variables
├── .storybook/                 # Storybook configuration
└── style-dictionary.config.js  # Style Dictionary config
```

## Design System Components

### CognitiveMeshButton
A fully-featured button component that demonstrates the design system:

- **Variants**: Primary, Secondary, Neutral, Semantic
- **Sizes**: Small, Medium, Large
- **States**: Default, Hover, Active, Disabled, Focus
- **Features**: Glow effects, smooth animations, accessibility support

## Contributing

1. **Design Tokens**: Add new tokens to the `tokens/` directory
2. **Components**: Create new components in `src/components/`
3. **Documentation**: Add Storybook stories for new components
4. **Build Tokens**: Run `npm run tokens` after token changes

## Design System Integration

### Using Design Tokens in Components
```css
/* Import generated tokens */
@import '../../../build/css/_variables.css';

.my-component {
  background: var(--color-cognitive-primary-neural);
  font-family: var(--typography-font-family-primary);
  padding: var(--spacing-md);
}
```

### Adding New Tokens
1. Edit token files in `tokens/` directory
2. Run `npm run tokens` to regenerate outputs
3. Import and use in your components

## License

MIT License - see LICENSE file for details.
