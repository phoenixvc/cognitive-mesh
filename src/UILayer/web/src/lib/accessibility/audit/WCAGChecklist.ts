/**
 * WCAG 2.1 AA compliance checklist for the Cognitive Mesh UILayer.
 *
 * This module defines a structured checklist of WCAG 2.1 success criteria
 * at both Level A and Level AA, along with their current compliance status
 * within the application. The checklist is used by the accessibility audit
 * tooling and can be rendered in compliance dashboards.
 */

/**
 * Represents a single WCAG 2.1 success criterion with its compliance status.
 */
export interface WCAGCriterion {
  /** The WCAG success criterion identifier (e.g., '1.1.1'). */
  id: string;
  /** Human-readable name of the criterion. */
  name: string;
  /** WCAG conformance level: 'A' (minimum) or 'AA' (enhanced). */
  level: 'A' | 'AA';
  /** Current compliance status for this criterion. */
  status: 'pass' | 'fail' | 'partial' | 'not-tested';
  /** Optional notes about the compliance status or implementation details. */
  notes?: string;
}

/**
 * Comprehensive WCAG 2.1 AA checklist covering all Level A and AA success criteria.
 *
 * Status meanings:
 * - `pass`: The criterion is fully satisfied across all tested components.
 * - `fail`: The criterion has known violations that need to be addressed.
 * - `partial`: Some components comply but others need remediation.
 * - `not-tested`: The criterion has not yet been evaluated.
 */
export const wcagChecklist: WCAGCriterion[] = [
  // Principle 1: Perceivable
  {
    id: '1.1.1',
    name: 'Non-text Content',
    level: 'A',
    status: 'pass',
    notes: 'All images have alt text. SVG visualizations include <title> and <desc> elements.',
  },
  {
    id: '1.2.1',
    name: 'Audio-only and Video-only (Prerecorded)',
    level: 'A',
    status: 'not-tested',
    notes: 'No audio/video content in current UILayer components.',
  },
  {
    id: '1.2.2',
    name: 'Captions (Prerecorded)',
    level: 'A',
    status: 'not-tested',
    notes: 'No video content in current UILayer components.',
  },
  {
    id: '1.2.3',
    name: 'Audio Description or Media Alternative (Prerecorded)',
    level: 'A',
    status: 'not-tested',
    notes: 'No video content in current UILayer components.',
  },
  {
    id: '1.2.4',
    name: 'Captions (Live)',
    level: 'AA',
    status: 'not-tested',
    notes: 'No live media in current UILayer components.',
  },
  {
    id: '1.2.5',
    name: 'Audio Description (Prerecorded)',
    level: 'AA',
    status: 'not-tested',
    notes: 'No video content in current UILayer components.',
  },
  {
    id: '1.3.1',
    name: 'Info and Relationships',
    level: 'A',
    status: 'pass',
    notes: 'Semantic HTML used throughout. Tables have headers, forms have labels, regions have landmarks.',
  },
  {
    id: '1.3.2',
    name: 'Meaningful Sequence',
    level: 'A',
    status: 'pass',
    notes: 'DOM order matches visual presentation order in all components.',
  },
  {
    id: '1.3.3',
    name: 'Sensory Characteristics',
    level: 'A',
    status: 'pass',
    notes: 'Instructions do not rely solely on shape, color, size, or visual location.',
  },
  {
    id: '1.3.4',
    name: 'Orientation',
    level: 'AA',
    status: 'pass',
    notes: 'No orientation lock. All layouts work in both portrait and landscape.',
  },
  {
    id: '1.3.5',
    name: 'Identify Input Purpose',
    level: 'AA',
    status: 'partial',
    notes: 'autocomplete attributes added to common form fields. Some custom inputs may need review.',
  },
  {
    id: '1.4.1',
    name: 'Use of Color',
    level: 'A',
    status: 'pass',
    notes: 'Color is never the sole means of conveying information. Status badges include text labels alongside color.',
  },
  {
    id: '1.4.2',
    name: 'Audio Control',
    level: 'A',
    status: 'not-tested',
    notes: 'No auto-playing audio in current UILayer components.',
  },
  {
    id: '1.4.3',
    name: 'Contrast (Minimum)',
    level: 'AA',
    status: 'pass',
    notes: 'Text contrast ratios meet 4.5:1 for normal text and 3:1 for large text in both themes.',
  },
  {
    id: '1.4.4',
    name: 'Resize Text',
    level: 'AA',
    status: 'pass',
    notes: 'Text can be resized up to 200% without loss of content or functionality.',
  },
  {
    id: '1.4.5',
    name: 'Images of Text',
    level: 'AA',
    status: 'pass',
    notes: 'No images of text used. All text is rendered as DOM text.',
  },
  {
    id: '1.4.10',
    name: 'Reflow',
    level: 'AA',
    status: 'partial',
    notes: 'Most components reflow correctly at 320px. D3 visualizations may require horizontal scrolling.',
  },
  {
    id: '1.4.11',
    name: 'Non-text Contrast',
    level: 'AA',
    status: 'pass',
    notes: 'UI components and graphical elements meet 3:1 contrast ratio against adjacent colors.',
  },
  {
    id: '1.4.12',
    name: 'Text Spacing',
    level: 'AA',
    status: 'pass',
    notes: 'Content remains visible and functional when text spacing is increased per WCAG requirements.',
  },
  {
    id: '1.4.13',
    name: 'Content on Hover or Focus',
    level: 'AA',
    status: 'pass',
    notes: 'Tooltips are dismissible, hoverable, and persistent. No content disappears unexpectedly.',
  },

  // Principle 2: Operable
  {
    id: '2.1.1',
    name: 'Keyboard',
    level: 'A',
    status: 'pass',
    notes: 'All interactive elements are keyboard accessible with tabindex and keyboard event handlers.',
  },
  {
    id: '2.1.2',
    name: 'No Keyboard Trap',
    level: 'A',
    status: 'pass',
    notes: 'FocusTrap component ensures modals can be exited with Escape. No other keyboard traps exist.',
  },
  {
    id: '2.1.4',
    name: 'Character Key Shortcuts',
    level: 'A',
    status: 'pass',
    notes: 'No single-character keyboard shortcuts are used.',
  },
  {
    id: '2.2.1',
    name: 'Timing Adjustable',
    level: 'A',
    status: 'pass',
    notes: 'No time limits are imposed on user interactions.',
  },
  {
    id: '2.2.2',
    name: 'Pause, Stop, Hide',
    level: 'A',
    status: 'pass',
    notes: 'Animated content respects prefers-reduced-motion. Loading states can be paused.',
  },
  {
    id: '2.3.1',
    name: 'Three Flashes or Below Threshold',
    level: 'A',
    status: 'pass',
    notes: 'No content flashes more than three times per second.',
  },
  {
    id: '2.4.1',
    name: 'Bypass Blocks',
    level: 'A',
    status: 'pass',
    notes: 'SkipNavigation component provides skip-to-content functionality.',
  },
  {
    id: '2.4.2',
    name: 'Page Titled',
    level: 'A',
    status: 'pass',
    notes: 'All pages have descriptive titles. Panels use aria-labelledby for section titles.',
  },
  {
    id: '2.4.3',
    name: 'Focus Order',
    level: 'A',
    status: 'pass',
    notes: 'Tab order follows logical reading order. FocusTrap manages modal focus order.',
  },
  {
    id: '2.4.4',
    name: 'Link Purpose (In Context)',
    level: 'A',
    status: 'pass',
    notes: 'All links have descriptive text or aria-label attributes.',
  },
  {
    id: '2.4.5',
    name: 'Multiple Ways',
    level: 'AA',
    status: 'partial',
    notes: 'Widget registry provides navigation. Search functionality available in audit views.',
  },
  {
    id: '2.4.6',
    name: 'Headings and Labels',
    level: 'AA',
    status: 'pass',
    notes: 'All sections have descriptive headings. Form inputs have associated labels.',
  },
  {
    id: '2.4.7',
    name: 'Focus Visible',
    level: 'AA',
    status: 'pass',
    notes: 'useFocusVisible hook provides keyboard-only focus indicators. Browser defaults preserved.',
  },
  {
    id: '2.5.1',
    name: 'Pointer Gestures',
    level: 'A',
    status: 'pass',
    notes: 'All multipoint gestures have single-pointer alternatives. D3 zoom supports both mouse wheel and pinch.',
  },
  {
    id: '2.5.2',
    name: 'Pointer Cancellation',
    level: 'A',
    status: 'pass',
    notes: 'Click actions fire on mouseup/pointerup, not mousedown. Drag operations can be cancelled.',
  },
  {
    id: '2.5.3',
    name: 'Label in Name',
    level: 'A',
    status: 'pass',
    notes: 'Accessible names contain or match the visible text labels.',
  },
  {
    id: '2.5.4',
    name: 'Motion Actuation',
    level: 'A',
    status: 'pass',
    notes: 'No motion-activated functionality in current components.',
  },

  // Principle 3: Understandable
  {
    id: '3.1.1',
    name: 'Language of Page',
    level: 'A',
    status: 'pass',
    notes: 'HTML lang attribute set on document root.',
  },
  {
    id: '3.1.2',
    name: 'Language of Parts',
    level: 'AA',
    status: 'partial',
    notes: 'Primary language set. Multi-language content in Localization layer uses lang attributes.',
  },
  {
    id: '3.2.1',
    name: 'On Focus',
    level: 'A',
    status: 'pass',
    notes: 'No unexpected context changes occur on focus.',
  },
  {
    id: '3.2.2',
    name: 'On Input',
    level: 'A',
    status: 'pass',
    notes: 'Form submissions require explicit user action. Filter changes do not trigger navigation.',
  },
  {
    id: '3.2.3',
    name: 'Consistent Navigation',
    level: 'AA',
    status: 'pass',
    notes: 'Navigation patterns are consistent across all widget panels.',
  },
  {
    id: '3.2.4',
    name: 'Consistent Identification',
    level: 'AA',
    status: 'pass',
    notes: 'Components with the same functionality use consistent labeling and icons.',
  },
  {
    id: '3.3.1',
    name: 'Error Identification',
    level: 'A',
    status: 'pass',
    notes: 'Errors use role="alert" and provide descriptive text. Error states are not color-only.',
  },
  {
    id: '3.3.2',
    name: 'Labels or Instructions',
    level: 'A',
    status: 'pass',
    notes: 'All form controls have visible labels or aria-label attributes with instructions.',
  },
  {
    id: '3.3.3',
    name: 'Error Suggestion',
    level: 'AA',
    status: 'partial',
    notes: 'API error messages include retry options. Form validation suggestions partially implemented.',
  },
  {
    id: '3.3.4',
    name: 'Error Prevention (Legal, Financial, Data)',
    level: 'AA',
    status: 'pass',
    notes: 'Authority override actions require confirmation modal. Consent flows are reversible.',
  },

  // Principle 4: Robust
  {
    id: '4.1.1',
    name: 'Parsing',
    level: 'A',
    status: 'pass',
    notes: 'JSX ensures valid HTML output. No duplicate IDs or unclosed elements.',
  },
  {
    id: '4.1.2',
    name: 'Name, Role, Value',
    level: 'A',
    status: 'pass',
    notes: 'All interactive elements have accessible names. ARIA roles and states used correctly.',
  },
  {
    id: '4.1.3',
    name: 'Status Messages',
    level: 'AA',
    status: 'pass',
    notes: 'LiveRegion component announces dynamic updates. Loading and error states use aria-live.',
  },
];

/**
 * Returns the count of criteria at each compliance status.
 *
 * @param checklist - The WCAG checklist to analyze. Defaults to the full checklist.
 * @returns An object mapping status values to their counts.
 */
export function getComplianceSummary(
  checklist: WCAGCriterion[] = wcagChecklist
): Record<WCAGCriterion['status'], number> {
  return checklist.reduce(
    (acc, criterion) => {
      acc[criterion.status] += 1;
      return acc;
    },
    { pass: 0, fail: 0, partial: 0, 'not-tested': 0 }
  );
}

/**
 * Filters the checklist to return only criteria with violations or partial compliance.
 *
 * @param checklist - The WCAG checklist to filter. Defaults to the full checklist.
 * @returns Array of criteria that need attention.
 */
export function getActionableItems(
  checklist: WCAGCriterion[] = wcagChecklist
): WCAGCriterion[] {
  return checklist.filter(
    (criterion) => criterion.status === 'fail' || criterion.status === 'partial'
  );
}
