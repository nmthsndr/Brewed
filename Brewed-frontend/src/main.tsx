import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { MantineProvider, createTheme } from '@mantine/core';
import { Notifications } from '@mantine/notifications';
import { AuthProvider } from './context/AuthProvider';
import CartProvider from './context/CartProvider';
import Routing from './routing/Routing';
import '@mantine/core/styles.css';
import '@mantine/notifications/styles.css';
import '@mantine/dates/styles.css';

const theme = createTheme({
  primaryColor: 'brown',
  colors: {
    brown: [
      '#F5E6D3',
      '#E6D1B3',
      '#D4A373',
      '#C69063',
      '#B87D53',
      '#8B4513',
      '#7A3C10',
      '#69330D',
      '#582A0B',
      '#472108'
    ]
  },
  defaultRadius: 'md',
  fontFamily: '"Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
  headings: {
    fontFamily: '"Playfair Display", Georgia, serif',
    fontWeight: '700',
  },
  components: {
    Button: {
      defaultProps: {
        radius: 'md',
      },
      styles: {
        root: {
          fontWeight: 600,
          letterSpacing: '0.01em',
          transition: 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
        },
      },
    },
    Card: {
      defaultProps: {
        radius: 'lg',
      },
      styles: {
        root: {
          transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
        },
      },
    },
    Paper: {
      defaultProps: {
        radius: 'lg',
      },
    },
    TextInput: {
      styles: {
        input: {
          transition: 'border-color 0.2s ease, box-shadow 0.2s ease',
        },
      },
    },
    PasswordInput: {
      styles: {
        input: {
          transition: 'border-color 0.2s ease, box-shadow 0.2s ease',
        },
      },
    },
    Modal: {
      defaultProps: {
        radius: 'lg',
        overlayProps: { backgroundOpacity: 0.4, blur: 4 },
      },
    },
    Table: {
      styles: {
        table: {
          fontSize: '0.875rem',
        },
      },
    },
    Badge: {
      styles: {
        root: {
          fontWeight: 600,
          letterSpacing: '0.02em',
          textTransform: 'uppercase' as const,
        },
      },
    },
    Pagination: {
      defaultProps: {
        color: 'brown',
        radius: 'md',
      },
    },
    Tabs: {
      defaultProps: {
        color: 'brown',
      },
    },
  },
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <MantineProvider theme={theme}>
      <Notifications position="top-right" />
      <BrowserRouter>
        <AuthProvider>
          <CartProvider>
            <Routing />
          </CartProvider>
        </AuthProvider>
      </BrowserRouter>
    </MantineProvider>
  </React.StrictMode>,
);