import React from 'react';
import './CognitiveMeshCard.module.css';

export interface CognitiveMeshCardProps {
  variant?: 'default' | 'elevated' | 'outlined' | 'gradient';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  title?: string;
  subtitle?: string;
  className?: string;
  onClick?: () => void;
}

export const CognitiveMeshCard: React.FC<CognitiveMeshCardProps> = ({
  variant = 'default',
  size = 'md',
  children,
  title,
  subtitle,
  className = '',
  onClick,
}) => {
  const baseClass = 'cognitive-mesh-card';
  const variantClass = `${baseClass}--${variant}`;
  const sizeClass = `${baseClass}--${size}`;
  const clickableClass = onClick ? `${baseClass}--clickable` : '';

  return (
    <div
      className={`${baseClass} ${variantClass} ${sizeClass} ${clickableClass} ${className}`}
      onClick={onClick}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={onClick ? (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onClick();
        }
      } : undefined}
    >
      {(title || subtitle) && (
        <div className={`${baseClass}__header`}>
          {title && <h3 className={`${baseClass}__title`}>{title}</h3>}
          {subtitle && <p className={`${baseClass}__subtitle`}>{subtitle}</p>}
        </div>
      )}
      <div className={`${baseClass}__content`}>
        {children}
      </div>
    </div>
  );
}; 