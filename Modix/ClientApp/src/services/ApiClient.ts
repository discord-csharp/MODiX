import Axios from 'axios';

const client = Axios.create
({
    baseURL: '/api/',
    timeout: 5000,
    withCredentials: true
});

export default client;