import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import './index.css';

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
  },
});

ReactDOM.render(
  <ThemeProvider theme={darkTheme}>
    <App/>
  </ThemeProvider>,
  document.getElementById('root')
);
