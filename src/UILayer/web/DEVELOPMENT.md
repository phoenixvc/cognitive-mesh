# CognitiveMeshUI Development Setup

This project is configured with a complete development environment using VS Code Dev Containers.

## 🚀 Quick Start

### Using Dev Containers (Recommended)

1. **Prerequisites:**
   - [VS Code](https://code.visualstudio.com/)
   - [Docker](https://docker.com/get-started)
   - [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

2. **Setup:**
   ```bash
   # Clone the repository
   git clone <your-repo-url>
   cd CognitiveMeshUI
   
   # Open in VS Code
   code .
   
   # When prompted, click "Reopen in Container"
   # Or use Command Palette: "Dev Containers: Reopen in Container"
   ```

3. **The container will automatically:**
   - Install Node.js 20
   - Install all npm dependencies
   - Configure VS Code with recommended extensions
   - Set up development tools

### Manual Setup (Alternative)

If you prefer not to use Dev Containers:

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Open http://localhost:3000
```

## 🛠️ Available Commands

### VS Code Tasks (Ctrl+Shift+P → "Tasks: Run Task")

- **Dev Server** - Start the Next.js development server
- **Build** - Create production build
- **Lint** - Run ESLint
- **Lint Fix** - Fix ESLint issues automatically
- **Type Check** - Run TypeScript compiler check
- **Clean** - Clean build artifacts
- **Install Dependencies** - Install npm packages

### NPM Scripts

```bash
# Development
npm run dev          # Start development server
npm run build        # Build for production
npm run start        # Start production server
npm run lint         # Run linting

# Utilities
npm run clean        # Clean build artifacts
```

## 🐛 Debugging

The workspace includes multiple debugging configurations:

- **Next.js: debug server-side** - Debug server-side code
- **Next.js: debug client-side** - Debug in Chrome
- **Next.js: debug full stack** - Debug both server and client
- **Debug Build** - Debug build process

Access via VS Code Debug panel (Ctrl+Shift+D).

## 🎨 Development Features

### Code Quality
- **ESLint** - Code linting with Next.js rules
- **Prettier** - Code formatting (auto-format on save)
- **TypeScript** - Type checking and IntelliSense

### Styling
- **Tailwind CSS** - Utility-first CSS framework
- **CSS Modules** - Scoped styling support
- **Custom Properties** - CSS variables for theming

### Extensions Included
- Prettier (code formatting)
- ESLint (linting)
- Tailwind CSS IntelliSense
- TypeScript support
- Auto Rename Tag
- Path Intellisense
- Error Lens (inline error display)
- GitHub Copilot (if available)

## 📁 Project Structure

```
src/
├── app/                 # Next.js App Router
│   ├── globals.css     # Global styles
│   ├── layout.tsx      # Root layout
│   └── page.tsx        # Home page
├── components/         # React components
│   ├── ui/            # Reusable UI components
│   └── ...            # Feature components
├── hooks/             # Custom React hooks
├── lib/               # Utility functions
└── types/             # TypeScript type definitions

components/            # Shared UI components (shadcn/ui)
public/               # Static assets
```

## 🔧 Configuration Files

- `.devcontainer/` - Dev container configuration
- `.vscode/` - VS Code workspace settings
- `tailwind.config.js` - Tailwind CSS configuration
- `tsconfig.json` - TypeScript configuration
- `next.config.js` - Next.js configuration

## 🌐 Port Forwarding

The dev container automatically forwards these ports:
- **3000** - Next.js development server
- **3001** - Storybook (if added)
- **5173** - Vite dev server (if needed)

## 🎯 Tips

1. **Auto-formatting:** Files are automatically formatted on save
2. **Auto-imports:** TypeScript imports are organized automatically
3. **Live reload:** Development server auto-reloads on changes
4. **Error highlighting:** Errors are highlighted inline with Error Lens
5. **File nesting:** Related files are grouped in the explorer

## 🐳 Container Details

The dev container uses:
- **Base Image:** `mcr.microsoft.com/devcontainers/typescript-node:1-20-bullseye`
- **Node.js:** Version 20
- **Features:** Git, GitHub CLI, Docker-in-Docker
- **User:** `node` (non-root for security)

## 🚨 Troubleshooting

### Container Issues
```bash
# Rebuild container
Ctrl+Shift+P → "Dev Containers: Rebuild Container"

# Reset container
Ctrl+Shift+P → "Dev Containers: Rebuild and Reopen in Container"
```

### Port Conflicts
If port 3000 is in use, Next.js will automatically use the next available port.

### Extension Issues
Extensions are automatically installed, but you can manually install via:
```bash
Ctrl+Shift+P → "Extensions: Install Extensions"
```

---

Happy coding! 🚀

# Development Guide

This guide covers development workflows for the Cognitive Mesh UI project, including the design system, component development, and documentation.

## Design System Workflow

### 1. Working with Design Tokens

The project uses **Style Dictionary** to manage design tokens across multiple platforms.

#### Token Structure
```
tokens/
├── colors.json          # Color tokens (primary, secondary, semantic)
├── typography.json      # Font families, sizes, weights, line heights
├── spacing.json         # Spacing scale
└── [new-tokens].json    # Additional token categories
```

#### Adding New Tokens
1. **Edit token files** in the `tokens/` directory
2. **Build tokens**: `npm run tokens`
3. **Use in components**: Import generated CSS variables

#### Example: Adding a new color token
```json
// tokens/colors.json
{
  "color": {
    "cognitive": {
      "accent": {
        "new": { "value": "#FF6B9D" }
      }
    }
  }
}
```

After running `npm run tokens`, use in CSS:
```css
.my-component {
  background: var(--color-cognitive-accent-new);
}
```

### 2. Component Development

#### Creating New Components
1. **Create component directory**:
   ```
   src/components/MyComponent/
   ├── MyComponent.tsx
   ├── MyComponent.module.css
   ├── index.ts
   └── MyComponent.stories.tsx
   ```

2. **Use design tokens** in CSS:
   ```css
   @import '../../../build/css/_variables.css';
   
   .my-component {
     font-family: var(--typography-font-family-primary);
     padding: var(--spacing-md);
     background: var(--color-cognitive-primary-neural);
   }
   ```

3. **Add Storybook stories** for documentation and testing

#### Component Guidelines
- Use TypeScript for type safety
- Implement proper accessibility (ARIA labels, keyboard navigation)
- Follow the existing component patterns
- Include hover, focus, and disabled states
- Use CSS modules for styling

### 3. Storybook Documentation

#### Running Storybook
```bash
npm run storybook
```
Access at: http://localhost:6006

#### Creating Stories
```typescript
// MyComponent.stories.tsx
import type { Meta, StoryObj } from '@storybook/react';
import { MyComponent } from './MyComponent';

const meta: Meta<typeof MyComponent> = {
  title: 'Design System/MyComponent',
  component: MyComponent,
  parameters: {
    layout: 'centered',
  },
  argTypes: {
    variant: {
      control: { type: 'select' },
      options: ['primary', 'secondary'],
    },
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof meta>;

export const Primary: Story = {
  args: {
    variant: 'primary',
    children: 'Primary Button',
  },
};
```

#### Story Best Practices
- Use descriptive story names
- Include all component variants
- Add interactive controls for props
- Document component usage with descriptions
- Include accessibility information

### 4. Development Commands

#### Essential Commands
```bash
# Development
npm run dev              # Start Next.js dev server
npm run storybook        # Start Storybook
npm run tokens           # Build design tokens

# Building
npm run build            # Build Next.js app
npm run build-storybook  # Build static Storybook

# Quality
npm run lint             # Run ESLint
```

#### Token Development Workflow
1. Edit tokens in `tokens/` directory
2. Run `npm run tokens` to regenerate
3. Check generated files in `build/` directory
4. Update components to use new tokens
5. Test in Storybook

### 5. File Structure Guidelines

#### Component Organization
```
src/components/
├── ComponentName/
│   ├── ComponentName.tsx      # Main component
│   ├── ComponentName.module.css # Styles
│   ├── index.ts               # Exports
│   └── ComponentName.stories.tsx # Documentation
```

#### Token Organization
```
tokens/
├── colors.json               # All color tokens
├── typography.json           # All typography tokens
├── spacing.json              # All spacing tokens
└── [category].json           # Other token categories
```

### 6. Design System Principles

#### Token Naming Convention
- Use kebab-case for CSS variables
- Group related tokens hierarchically
- Use semantic names over visual descriptions
- Include platform-specific variants when needed

#### Component Design Principles
- **Consistency**: Use design tokens for all styling
- **Accessibility**: Implement proper ARIA and keyboard support
- **Responsive**: Design for all screen sizes
- **Performance**: Optimize for fast rendering
- **Maintainability**: Write clean, documented code

### 7. Testing Strategy

#### Component Testing
- Use Storybook for visual testing
- Test all variants and states
- Verify accessibility with screen readers
- Test responsive behavior
- Validate design token usage

#### Token Testing
- Verify token generation works correctly
- Test token usage across components
- Validate CSS custom properties
- Check cross-platform compatibility

### 8. Troubleshooting

#### Common Issues

**Tokens not updating in components**
- Run `npm run tokens` to regenerate
- Check import paths in CSS files
- Verify token file syntax

**Storybook not loading**
- Check for TypeScript errors
- Verify component imports
- Restart Storybook server

**Style conflicts**
- Use CSS modules to avoid conflicts
- Check CSS specificity
- Verify token variable names

#### Getting Help
- Check the generated token files in `build/`
- Review Storybook documentation
- Test components in isolation
- Use browser dev tools to inspect styles
