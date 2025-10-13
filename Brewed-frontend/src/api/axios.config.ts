import axios from 'axios';
import { tokenKeyName } from "../constants/constants";

const baseURL = `${import.meta.env.VITE_REST_API_URL || "http://localhost:5243"}`;

const axiosInstance = axios.create({
    baseURL
});

axiosInstance.interceptors.request.use(
    config => {
        const token = localStorage.getItem(tokenKeyName);
        if (token) {
            config.headers["Authorization"] = `Bearer ${token}`;
        }
        return config;
    },
    error => {
        return Promise.reject(error);
    }
);

export default axiosInstance;