<script setup>
import { PackageCheck, CheckCircle2, Clock3, AlertTriangle } from 'lucide-vue-next';

defineProps({
  counts: {
    type: Object,
    required: true,
  },
});

const cards = [
  { key: 'expected', label: 'Expected', icon: PackageCheck, tone: 'blue' },
  { key: 'received', label: 'Received', icon: CheckCircle2, tone: 'green' },
  { key: 'pending', label: 'Pending', icon: Clock3, tone: 'orange' },
  { key: 'discrepancies', label: 'Discrepancies', icon: AlertTriangle, tone: 'red' },
];
</script>

<template>
  <div class="summary-grid">
    <article v-for="card in cards" :key="card.key" class="summary-card">
      <span class="summary-icon" :class="card.tone"><component :is="card.icon" :size="18" /></span>
      <span class="summary-copy">
        <small>{{ card.label }}</small>
        <strong>{{ counts[card.key] ?? 0 }}</strong>
      </span>
    </article>
  </div>
</template>
