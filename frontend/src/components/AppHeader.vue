<script setup>
import { FlaskConical, RefreshCw } from 'lucide-vue-next';

defineProps({ refreshing: { type: Boolean, default: false } });
defineEmits(['refresh']);
</script>

<template>
  <header class="app-header">
    <a class="brand" href="#" aria-label="SpecimenFlow home">
      <span class="brand-icon"><FlaskConical :size="22" /></span>
      <span>
        <strong>SpecimenFlow</strong>
        <small>Laboratory Information System</small>
      </span>
    </a>

    <div class="header-actions">
      <span class="lab-indicator"><span class="online-dot" />Lab Alpha</span>
      <button class="icon-button" type="button" title="Refresh data" :disabled="refreshing" @click="$emit('refresh')">
        <RefreshCw :size="17" :class="{ spinning: refreshing }" />
      </button>
    </div>
  </header>
</template>
