import React from 'react';
import { CognitiveMeshButton } from '../CognitiveMeshButton';
import { CognitiveMeshCard } from '../CognitiveMeshCard';
import './DesignSystemShowcase.module.css';

export const DesignSystemShowcase: React.FC = () => {
  return (
    <div className="design-system-showcase">
      <header className="showcase-header">
        <h1 className="showcase-title">Cognitive Mesh Design System</h1>
        <p className="showcase-subtitle">
          A comprehensive design system built with Style Dictionary and React
        </p>
      </header>

      <section className="showcase-section">
        <h2 className="section-title">Color Palette</h2>
        <div className="color-grid">
          <div className="color-group">
            <h3>Primary Colors</h3>
            <div className="color-swatches">
              <div className="color-swatch neural">
                <span className="color-name">Neural</span>
                <span className="color-value">#00D4FF</span>
              </div>
              <div className="color-swatch synaptic">
                <span className="color-name">Synaptic</span>
                <span className="color-value">#0066CC</span>
              </div>
              <div className="color-swatch axonal">
                <span className="color-name">Axonal</span>
                <span className="color-value">#003366</span>
              </div>
            </div>
          </div>
          
          <div className="color-group">
            <h3>Secondary Colors</h3>
            <div className="color-swatches">
              <div className="color-swatch dendritic">
                <span className="color-name">Dendritic</span>
                <span className="color-value">#FF6B35</span>
              </div>
              <div className="color-swatch myelin">
                <span className="color-name">Myelin</span>
                <span className="color-value">#FFD93D</span>
              </div>
              <div className="color-swatch synapse">
                <span className="color-name">Synapse</span>
                <span className="color-value">#6BCF7F</span>
              </div>
            </div>
          </div>
          
          <div className="color-group">
            <h3>Neutral Colors</h3>
            <div className="color-swatches">
              <div className="color-swatch cortex">
                <span className="color-name">Cortex</span>
                <span className="color-value">#1A1A2E</span>
              </div>
              <div className="color-swatch matter">
                <span className="color-name">Matter</span>
                <span className="color-value">#16213E</span>
              </div>
              <div className="color-swatch fluid">
                <span className="color-name">Fluid</span>
                <span className="color-value">#0F3460</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="showcase-section">
        <h2 className="section-title">Typography</h2>
        <div className="typography-examples">
          <div className="font-family">
            <h3>Font Families</h3>
            <div className="font-examples">
              <div className="font-example primary">
                <span className="font-label">Primary (Inter)</span>
                <p>The quick brown fox jumps over the lazy dog</p>
              </div>
              <div className="font-example mono">
                <span className="font-label">Mono (JetBrains Mono)</span>
                <p>The quick brown fox jumps over the lazy dog</p>
              </div>
              <div className="font-example display">
                <span className="font-label">Display (Orbitron)</span>
                <p>THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG</p>
              </div>
            </div>
          </div>
          
          <div className="font-sizes">
            <h3>Font Sizes</h3>
            <div className="size-examples">
              <div className="size-example xs">Extra Small (0.75rem)</div>
              <div className="size-example sm">Small (0.875rem)</div>
              <div className="size-example base">Base (1rem)</div>
              <div className="size-example lg">Large (1.125rem)</div>
              <div className="size-example xl">Extra Large (1.25rem)</div>
              <div className="size-example 2xl">2XL (1.5rem)</div>
              <div className="size-example 3xl">3XL (1.875rem)</div>
            </div>
          </div>
        </div>
      </section>

      <section className="showcase-section">
        <h2 className="section-title">Spacing Scale</h2>
        <div className="spacing-examples">
          <div className="spacing-item xs">
            <span className="spacing-label">XS (0.25rem)</span>
            <div className="spacing-visual"></div>
          </div>
          <div className="spacing-item sm">
            <span className="spacing-label">SM (0.5rem)</span>
            <div className="spacing-visual"></div>
          </div>
          <div className="spacing-item md">
            <span className="spacing-label">MD (1rem)</span>
            <div className="spacing-visual"></div>
          </div>
          <div className="spacing-item lg">
            <span className="spacing-label">LG (1.5rem)</span>
            <div className="spacing-visual"></div>
          </div>
          <div className="spacing-item xl">
            <span className="spacing-label">XL (2rem)</span>
            <div className="spacing-visual"></div>
          </div>
        </div>
      </section>

      <section className="showcase-section">
        <h2 className="section-title">Components</h2>
        
        <div className="component-group">
          <h3>Buttons</h3>
          <div className="component-examples">
            <div className="button-group">
              <h4>Variants</h4>
              <div className="button-row">
                <CognitiveMeshButton variant="primary">Primary</CognitiveMeshButton>
                <CognitiveMeshButton variant="secondary">Secondary</CognitiveMeshButton>
                <CognitiveMeshButton variant="neutral">Neutral</CognitiveMeshButton>
                <CognitiveMeshButton variant="semantic">Semantic</CognitiveMeshButton>
              </div>
            </div>
            
            <div className="button-group">
              <h4>Sizes</h4>
              <div className="button-row">
                <CognitiveMeshButton variant="primary" size="sm">Small</CognitiveMeshButton>
                <CognitiveMeshButton variant="primary" size="md">Medium</CognitiveMeshButton>
                <CognitiveMeshButton variant="primary" size="lg">Large</CognitiveMeshButton>
              </div>
            </div>
          </div>
        </div>

        <div className="component-group">
          <h3>Cards</h3>
          <div className="component-examples">
            <div className="card-grid">
              <CognitiveMeshCard 
                variant="default" 
                title="Default Card"
                subtitle="Neutral styling"
              >
                Standard card with subtle hover effects.
              </CognitiveMeshCard>
              
              <CognitiveMeshCard 
                variant="elevated" 
                title="Elevated Card"
                subtitle="Prominent shadow"
              >
                Card with elevated appearance and lift animation.
              </CognitiveMeshCard>
              
              <CognitiveMeshCard 
                variant="outlined" 
                title="Outlined Card"
                subtitle="Colored border"
              >
                Card with neural blue border and hover glow.
              </CognitiveMeshCard>
              
              <CognitiveMeshCard 
                variant="gradient" 
                title="Gradient Card"
                subtitle="Neural background"
              >
                Card with beautiful gradient and glow effects.
              </CognitiveMeshCard>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}; 