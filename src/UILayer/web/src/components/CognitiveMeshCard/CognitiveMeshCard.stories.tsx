import type { Meta, StoryObj } from '@storybook/react';
import { CognitiveMeshCard } from './CognitiveMeshCard';

const meta: Meta<typeof CognitiveMeshCard> = {
  title: 'Design System/CognitiveMeshCard',
  component: CognitiveMeshCard,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: 'A versatile card component that uses the Cognitive Mesh design system tokens. Features multiple variants, sizes, and interactive states with sci-fi inspired styling.',
      },
    },
  },
  argTypes: {
    variant: {
      control: { type: 'select' },
      options: ['default', 'elevated', 'outlined', 'gradient'],
      description: 'The visual style variant of the card',
    },
    size: {
      control: { type: 'select' },
      options: ['sm', 'md', 'lg'],
      description: 'The size of the card',
    },
    onClick: { action: 'clicked' },
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  args: {
    variant: 'default',
    size: 'md',
    title: 'Default Card',
    subtitle: 'A standard card with neutral styling',
    children: 'This is the content of the card. It can contain any React elements.',
  },
};

export const Elevated: Story = {
  args: {
    variant: 'elevated',
    size: 'md',
    title: 'Elevated Card',
    subtitle: 'A card with elevated shadow and hover effects',
    children: 'This card has a more prominent shadow and lifts on hover.',
  },
};

export const Outlined: Story = {
  args: {
    variant: 'outlined',
    size: 'md',
    title: 'Outlined Card',
    subtitle: 'A card with a colored border',
    children: 'This card uses an outlined style with a neural blue border.',
  },
};

export const Gradient: Story = {
  args: {
    variant: 'gradient',
    size: 'md',
    title: 'Gradient Card',
    subtitle: 'A card with neural gradient background',
    children: 'This card features a beautiful neural gradient with glow effects.',
  },
};

export const Small: Story = {
  args: {
    variant: 'default',
    size: 'sm',
    title: 'Small Card',
    children: 'A compact card for tight spaces.',
  },
};

export const Large: Story = {
  args: {
    variant: 'default',
    size: 'lg',
    title: 'Large Card',
    subtitle: 'A spacious card for important content',
    children: 'This larger card provides more space for content and better visual hierarchy.',
  },
};

export const Clickable: Story = {
  args: {
    variant: 'elevated',
    size: 'md',
    title: 'Clickable Card',
    subtitle: 'Click me to see the interaction',
    children: 'This card is clickable and will trigger an action when clicked.',
    onClick: () => alert('Card clicked!'),
  },
};

export const AllVariants: Story = {
  render: () => (
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '2rem' }}>
      <CognitiveMeshCard variant="default" title="Default" subtitle="Neutral styling">
        Standard card with subtle hover effects.
      </CognitiveMeshCard>
      <CognitiveMeshCard variant="elevated" title="Elevated" subtitle="Prominent shadow">
        Card with elevated appearance and lift animation.
      </CognitiveMeshCard>
      <CognitiveMeshCard variant="outlined" title="Outlined" subtitle="Colored border">
        Card with neural blue border and hover glow.
      </CognitiveMeshCard>
      <CognitiveMeshCard variant="gradient" title="Gradient" subtitle="Neural background">
        Card with beautiful gradient and glow effects.
      </CognitiveMeshCard>
    </div>
  ),
  parameters: {
    docs: {
      description: {
        story: 'All card variants displayed together for comparison.',
      },
    },
  },
};

export const AllSizes: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '1rem', alignItems: 'flex-start' }}>
      <CognitiveMeshCard variant="default" size="sm" title="Small">
        Compact card for tight spaces.
      </CognitiveMeshCard>
      <CognitiveMeshCard variant="default" size="md" title="Medium">
        Standard size card with balanced proportions.
      </CognitiveMeshCard>
      <CognitiveMeshCard variant="default" size="lg" title="Large">
        Spacious card for important content.
      </CognitiveMeshCard>
    </div>
  ),
  parameters: {
    docs: {
      description: {
        story: 'All card sizes displayed together for comparison.',
      },
    },
  },
}; 