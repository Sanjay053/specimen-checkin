<script setup>
import { computed, ref } from 'vue';
import { Search, ChevronRight, ClipboardList } from 'lucide-vue-next';
import StatusBadge from './StatusBadge.vue';

const props = defineProps({
  manifests: { type: Array, default: () => [] },
  selectedId: { type: [Number, String], default: null },
  loading: { type: Boolean, default: false },
});

defineEmits(['select']);

const search = ref('');
const filter = ref('All');
const filteredManifests = computed(() => props.manifests.filter((manifest) => {
  const code = String(manifest.code ?? manifest.manifestCode ?? '').toLowerCase();
  const matchesSearch = code.includes(search.value.trim().toLowerCase());
  const matchesFilter = filter.value === 'All' || String(manifest.status).toLowerCase() === filter.value.toLowerCase();
  return matchesSearch && matchesFilter;
}));

function progress(manifest) {
  const expected = Number(manifest.expected ?? manifest.expectedCount ?? 0);
  const received = Number(manifest.received ?? manifest.receivedCount ?? 0);
  return expected > 0 ? Math.min(100, Math.round((received / expected) * 100)) : 0;
}
</script>

<template>
  <aside class="manifest-panel">
    <div class="panel-heading">
      <div><h2>Incoming manifests</h2><p>{{ manifests.length }} manifests available</p></div>
      <span class="count-pill">{{ manifests.length }}</span>
    </div>

    <label class="search-box">
      <Search :size="16" />
      <input v-model="search" type="search" placeholder="Search manifest..." aria-label="Search manifests" />
    </label>

    <div class="filter-tabs" role="group" aria-label="Filter manifests">
      <button v-for="item in ['All', 'Open', 'Closed']" :key="item" type="button" :class="{ active: filter === item }" @click="filter = item">{{ item }}</button>
    </div>

    <div v-if="loading" class="side-state"><span class="spinner" />Loading manifests...</div>
    <div v-else-if="filteredManifests.length === 0" class="side-state">
      <ClipboardList :size="28" /><span>No manifests found</span>
    </div>

    <div v-else class="manifest-list">
      <button
        v-for="manifest in filteredManifests"
        :key="manifest.id"
        type="button"
        class="manifest-card"
        :class="{ selected: String(selectedId) === String(manifest.id) }"
        @click="$emit('select', manifest.id)"
      >
        <span class="manifest-card-top">
          <strong>{{ manifest.code ?? manifest.manifestCode ?? `Manifest #${manifest.id}` }}</strong>
          <StatusBadge :status="manifest.status" />
        </span>
        <span class="manifest-meta">{{ manifest.expected ?? manifest.expectedCount ?? 0 }} expected specimens</span>
        <span class="progress-row">
          <span class="progress-track"><span class="progress-fill" :style="{ width: `${progress(manifest)}%` }" /></span>
          <small>{{ manifest.received ?? manifest.receivedCount ?? 0 }}/{{ manifest.expected ?? manifest.expectedCount ?? 0 }}</small>
        </span>
        <span class="manifest-bottom">
          <small>{{ manifest.pending ?? manifest.pendingCount ?? 0 }} pending</small>
          <small>{{ manifest.discrepancies ?? manifest.discrepancyCount ?? 0 }} discrepancies</small>
          <ChevronRight :size="15" />
        </span>
      </button>
    </div>
  </aside>
</template>
