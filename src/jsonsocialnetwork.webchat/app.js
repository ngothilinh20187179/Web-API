import express from 'express';
import { createProxyMiddleware } from 'http-proxy-middleware';
import dotenv from 'dotenv';

dotenv.config();
const app = express();
app.use(express.static('client/build'));
app.use(
    '/',
    createProxyMiddleware({
        target: process.env.PROXY,
        changeOrigin: true,
        ws: true,
        secure: false
    })
);

const port = process.env.PORT;
console.log('WebChat server on port: ' + port);
app.listen(port);