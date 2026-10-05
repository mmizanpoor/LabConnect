/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  important: true,
  theme: {
    fontFamily: {
      sans: ['"Estedad-FD"', 'Tahoma', 'sans-serif'],
    },
    extend: {
      colors: {
        brand: {
          50: '#eef2ff',
          500: '#6366f1',
          600: '#4f46e5',
          700: '#4338ca',
        },
        lab: {
          DEFAULT: '#527baa',
          soft: '#eef3f9',
          border: '#c9d7ea',
          hover: '#f8fbff',
          header: '#e8eef6',
        },
      },
    },
  },
  plugins: [],
};
