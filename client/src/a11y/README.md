# Portable Accessibility Kit (a11y)

Framework-light accessibility assistant you can **copy into another SPA** (React preferred for the UI shell; core works without React).

Inspired by commercial widgets (e.g. Eye-Able) but **self-owned lab code** — no third-party license.

## What’s in the folder

| File | Role |
|------|------|
| `core.ts` | Prefs, localStorage, `applyA11yPrefs`, TTS helpers, `createA11yController` |
| `a11y-assistant.css` | FAB + panel + white icon tiles + `data-a11y-*` hooks (no panel scrollbars) |
| `AccessibilityAssistant.tsx` | React dock UI with per-action SVG icons |
| `index.ts` | Public exports |

## Features

- Light / dark theme (`data-theme`)
- High contrast (`data-contrast`)
- Font scale 100–160% (`--font-scale`)
- Blue-light warmth overlay
- Hide images / videos
- Underline links
- Reduce motion
- Color-blindness SVG filters (protanopia / deuteranopia / tritanopia)
- Page read-aloud via `speechSynthesis` (lab quality)
- Persist + reset + hide assistant

## Drop into another project (React + Vite)

1. Copy `client/src/a11y/` as e.g. `src/a11y/`.
2. Ensure host CSS uses `html { font-size: calc(16px * var(--font-scale, 1)); }` (or equivalent) and optional `[data-theme="dark"]` / `[data-contrast="high"]` tokens.
3. Boot prefs **before** first paint (avoids flash):

```ts
import { applyA11yPrefs, readA11yPrefs, DEFAULT_A11Y_KEYS } from './a11y'

applyA11yPrefs(readA11yPrefs(DEFAULT_A11Y_KEYS))
```

4. Mount the assistant once near the app root:

```tsx
import { AccessibilityAssistant, DEFAULT_A11Y_KEYS } from './a11y'

<AccessibilityAssistant
  keys={DEFAULT_A11Y_KEYS}
  speechLang="en-US"
  labels={{
    toggle: 'Accessibility tools',
    title: 'Accessibility',
    close: 'Close',
    light: 'Light',
    dark: 'Dark',
    fontUp: 'Increase text',
    fontDown: 'Decrease text',
    contrast: 'High contrast',
    blueFilter: 'Blue filter',
    hideImages: 'Hide images',
    underlineLinks: 'Underline links',
    reduceMotion: 'Reduce motion',
    colorFilter: 'Color profile',
    colorNone: 'None',
    colorProtanopia: 'Protanopia',
    colorDeuteranopia: 'Deuteranopia',
    colorTritanopia: 'Tritanopia',
    speak: 'Read page',
    stopSpeak: 'Stop reading',
    hideAssistant: 'Hide assistant',
    reset: 'Reset',
  }}
/>
```

5. No npm package required — plain TypeScript + CSS.

## Non-React host

Use only `core.ts`:

```ts
import { createA11yController, DEFAULT_A11Y_KEYS } from './a11y/core'

const a11y = createA11yController({ keys: DEFAULT_A11Y_KEYS })
a11y.boot()
a11y.setPrefs({ contrast: true, fontScale: 125 })
```

Build your own buttons; keep `a11y-assistant.css` (or copy the `html[data-a11y-*]` rules).

## Storage keys

- **New projects:** `DEFAULT_A11Y_KEYS` (`a11y-theme`, …).
- **BankApp:** `BANKAPP_A11Y_KEYS` (legacy `bankapp-*` keys so existing prefs keep working).

## Theming the FAB

Override `--a11y-fab` on `:root` / `[data-theme="dark"]`.

## Limits (lab)

- TTS is browser `speechSynthesis`, not a screen-reader replacement.
- Color filters are approximate SVG matrices.
- Not a WCAG audit tool — it helps users adjust presentation.

## BankApp docs

Turkish learning chapter: `docs/learning/tr/34-a11y-portable-kit.md`.
