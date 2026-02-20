import type { Meta, StoryObj } from '@storybook/react';
import { CognitiveMeshButton } from './CognitiveMeshButton';

const meta: Meta<typeof CognitiveMeshButton> = {
  title: 'Design System/CognitiveMeshButton',
  component: CognitiveMeshButton,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: 'A button component that uses the Cognitive Mesh design system tokens. Features multiple variants, sizes, and interactive states with sci-fi inspired styling.',
      },
    },
  },
  argTypes: {
    variant: {
      control: { type: 'select' },
      options: ['primary', 'secondary', 'neutral', 'semantic'],
      description: 'The visual style variant of the button',
    },
    size: {
      control: { type: 'select' },
      options: ['sm', 'md', 'lg'],
      description: 'The size of the button',
    },
    disabled: {
      control: { type: 'boolean' },
      description: 'Whether the button is disabled',
    },
    onClick: { action: 'clicked' },
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof meta>;

export const Primary: Story = {
  args: {
    variant: 'primary',
    size: 'md',
    children: 'Primary Button',
  },
};

export const Secondary: Story = {
  args: {
    variant: 'secondary',
    size: 'md',
    children: 'Secondary Button',
  },
};

export const Neutral: Story = {
  args: {
    variant: 'neutral',
    size: 'md',
    children: 'Neutral Button',
  },
};

export const Semantic: Story = {
  args: {
    variant: 'semantic',
    size: 'md',
    children: 'Semantic Button',
  },
};

export const Small: Story = {
  args: {
    variant: 'primary',
    size: 'sm',
    children: 'Small Button',
  },
};

export const Large: Story = {
  args: {
    variant: 'primary',
    size: 'lg',
    children: 'Large Button',
  },
};

export const Disabled: Story = {
  args: {
    variant: 'primary',
    size: 'md',
    children: 'Disabled Button',
    disabled: true,
  },
};

export const AllVariants: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
      <CognitiveMeshButton variant="primary">Primary</CognitiveMeshButton>
      <CognitiveMeshButton variant="secondary">Secondary</CognitiveMeshButton>
      <CognitiveMeshButton variant="neutral">Neutral</CognitiveMeshButton>
      <CognitiveMeshButton variant="semantic">Semantic</CognitiveMeshButton>
    </div>
  ),
  parameters: {
    docs: {
      description: {
        story: 'All button variants displayed together for comparison.',
      },
    },
  },
};

export const AllSizes: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
      <CognitiveMeshButton variant="primary" size="sm">Small</CognitiveMeshButton>
      <CognitiveMeshButton variant="primary" size="md">Medium</CognitiveMeshButton>
      <CognitiveMeshButton variant="primary" size="lg">Large</CognitiveMeshButton>
    </div>
  ),
  parameters: {
    docs: {
      description: {
        story: 'All button sizes displayed together for comparison.',
      },
    },
  },
}; 