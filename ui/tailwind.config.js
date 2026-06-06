/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        ink: {
          950: '#06090f',
          900: '#0a0e15',
          850: '#0e131c',
          800: '#13181f',
          700: '#1a212c',
          600: '#252e3d',
        },
        fog: {
          50:  '#e8e9ec',
          200: '#c9ced6',
          400: '#8b94a3',
          500: '#6b7283',
        },
        signal: {
          // Electric cyan — the "connected" state, accent throughout
          cyan: '#06b6d4',
          cyanDim: '#0891b2',
          // Warm amber for warnings
          amber: '#f59e0b',
          // Magenta for input/button press flashes
          magenta: '#ec4899',
        },
      },
      fontFamily: {
        sans: ['Geist', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['"Geist Mono"', 'ui-monospace', 'monospace'],
      },
      letterSpacing: {
        wider2: '0.18em',
      },
      boxShadow: {
        card: '0 1px 0 rgba(255,255,255,0.04) inset, 0 24px 60px -30px rgba(0,0,0,0.6)',
        glow: '0 0 30px -8px rgba(6,182,212,0.55)',
        glowAmber: '0 0 24px -6px rgba(245,158,11,0.55)',
      },
      backgroundImage: {
        grain: "url(\"data:image/svg+xml;utf8,<svg viewBox='0 0 240 240' xmlns='http://www.w3.org/2000/svg'><filter id='n'><feTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='2' stitchTiles='stitch'/><feColorMatrix values='0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  0 0 0 0.06 0'/></filter><rect width='100%' height='100%' filter='url(%23n)'/></svg>\")",
      },
      animation: {
        'pulse-slow': 'pulse-slow 3s ease-in-out infinite',
      },
      keyframes: {
        'pulse-slow': {
          '0%, 100%': { opacity: '0.6' },
          '50%': { opacity: '1' },
        },
      },
    },
  },
  plugins: [],
};
