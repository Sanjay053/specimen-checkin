<script setup>
import { computed } from 'vue';

const props = defineProps({
  status: { type: String, default: 'Unknown' },
});

const normalized = computed(() => String(props.status || 'unknown').toLowerCase().replace(/[^a-z0-9]+/g, '-'));
</script>

<template>
  <span class="status-badge" :class="normalized">
    <span class="status-dot" />
    {{ status || 'Unknown' }}
  </span>
</template>
