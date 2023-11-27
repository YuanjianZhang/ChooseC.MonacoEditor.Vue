<script setup>
    import { inject } from 'vue';
    import Layout from './components/EditorLayout.vue';
    const address = ''//inject('$address');
</script>
<template>
    <Layout :baseAddress="address" />
</template>