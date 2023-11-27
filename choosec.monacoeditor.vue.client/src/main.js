import './assets/style.css'

import { createApp } from 'vue'
import App from './App.vue'

import { ElSelect, ElInput, ElButton, ElRow, ElCol } from 'element-plus'
import 'element-plus/es/components/select/style/css'
import 'element-plus/es/components/input/style/css'
import 'element-plus/es/components/button/style/css'
import 'element-plus/es/components/row/style/css'
import 'element-plus/es/components/col/style/css'


const app = createApp(App);
app.use(ElSelect).use(ElInput).use(ElButton).use(ElRow).use(ElCol);
app.provide('$address', 'http://localhost:5108');
app.mount('#app');
