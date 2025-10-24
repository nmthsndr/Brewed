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
  fontFamily: 'system-ui, -apple-system, sans-serif',
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