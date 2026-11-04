<script setup>
import { CheckCircle2, XCircle, FlaskConical } from 'lucide-vue-next';
import StatusBadge from './StatusBadge.vue';

defineProps({
  specimens: { type: Array, default: () => [] },
  disabled: { type: Boolean, default: false },
  actionLoading: { type: Boolean, default: false },
});

defineEmits(['receive', 'flag']);

function specimenName(specimen) {
  return specimen.accessionNumber ?? specimen.specimenCode ?? specimen.code ?? specimen.barcode ?? specimen.name ?? `Specimen #${specimen.id}`;
}

function specimenType(specimen) {
  return specimen.specimenType ?? specimen.type ?? specimen.testType ?? specimen.category ?? 'Specimen';
}
</script>

<template>
  <div class="table-wrapper">
    <table>
      <thead><tr><th>SPECIMEN</th><th>TYPE</th><th>STATUS</th><th class="actions-heading">ACTIONS</th></tr></thead>
      <tbody>
        <tr v-for="specimen in specimens" :key="specimen.id">
          <td><strong>{{ specimenName(specimen) }}</strong><small>ID: {{ specimen.id }}</small></td>
          <td><span class="type-cell"><FlaskConical :size="14" />{{ specimenType(specimen) }}</span></td>
          <td><StatusBadge :status="specimen.status" /></td>
          <td>
            <div v-if="String(specimen.status).toLowerCase() === 'pending'" class="table-actions">
              <button class="action-button receive" type="button" :disabled="disabled || actionLoading" @click="$emit('receive', specimen.id)"><CheckCircle2 :size="14" />Receive</button>
              <button class="action-button flag" type="button" :disabled="disabled || actionLoading" @click="$emit('flag', specimen.id)"><XCircle :size="14" />Missing</button>
            </div>
            <span v-else class="completed-text">{{ specimen.status }}</span>
          </td>
        </tr>
        <tr v-if="specimens.length === 0"><td colspan="4" class="empty-row">No specimens found for this manifest.</td></tr>
      </tbody>
    </table>
  </div>
</template>
