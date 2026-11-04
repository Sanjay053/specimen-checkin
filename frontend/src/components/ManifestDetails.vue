<script setup>
import { RefreshCw, CheckCircle2, AlertTriangle, ClipboardCheck } from 'lucide-vue-next';
import SummaryCards from './SummaryCards.vue';
import SpecimenTable from './SpecimenTable.vue';
import StatusBadge from './StatusBadge.vue';

defineProps({
  manifest: { type: Object, default: null },
  specimens: { type: Array, default: () => [] },
  discrepancies: { type: Array, default: () => [] },
  counts: { type: Object, required: true },
  loading: { type: Boolean, default: false },
  actionLoading: { type: Boolean, default: false },
});

defineEmits(['refresh', 'receive', 'flag', 'resolve', 'close']);

function discrepancyText(item) {
  return item.description ?? item.reason ?? item.message ?? item.type ?? 'Missing specimen';
}
</script>

<template>
  <section class="details-panel">
    <div v-if="loading" class="center-state"><span class="spinner" />Loading manifest details...</div>
    <div v-else-if="!manifest" class="center-state">
      <ClipboardCheck :size="36" />
      <h3>Select a manifest</h3>
      <p>Choose a manifest from the worklist to view its specimens.</p>
    </div>

    <template v-else>
      <div class="detail-heading">
        <div>
          <span class="eyebrow">MANIFEST DETAILS</span>
          <div class="title-row"><h2>{{ manifest.code ?? manifest.manifestCode ?? `Manifest #${manifest.id}` }}</h2><StatusBadge :status="manifest.status" /></div>
          <p>Review specimens and reconcile shipment contents.</p>
        </div>
        <button class="secondary-button" type="button" :disabled="loading" @click="$emit('refresh')"><RefreshCw :size="15" />Refresh</button>
      </div>

      <SummaryCards :counts="counts" />

      <div class="section-heading"><div><h3>Specimen inventory</h3><p>{{ specimens.length }} specimens in this manifest</p></div><span class="count-pill">{{ specimens.length }}</span></div>
      <SpecimenTable :specimens="specimens" :disabled="manifest.status !== 'Open'" :action-loading="actionLoading" @receive="$emit('receive', $event)" @flag="$emit('flag', $event)" />

      <section class="discrepancy-section">
        <div class="section-heading"><div><h3>Discrepancies</h3><p>Review and resolve missing specimen reports.</p></div><span class="count-pill">{{ counts.openDiscrepancies }} open</span></div>

        <div v-if="discrepancies.length === 0" class="no-discrepancies"><CheckCircle2 :size="18" />No discrepancies reported.</div>
        <article v-for="item in discrepancies" :key="item.id" class="discrepancy-item">
          <span class="discrepancy-icon"><AlertTriangle :size="17" /></span>
          <span class="discrepancy-info"><strong>{{ discrepancyText(item) }}</strong><small>Discrepancy #{{ item.id }} · {{ item.status }}</small></span>
          <button v-if="item.status === 'Open'" class="action-button receive" type="button" :disabled="actionLoading || manifest.status !== 'Open'" @click="$emit('resolve', item.id)">Resolve</button>
          <span v-else class="completed-text">Resolved</span>
        </article>
      </section>

      <footer class="footer-actions">
        <!-- <div><strong>Manifest reconciliation</strong><p>{{ counts.pending }} pending specimens · {{ counts.openDiscrepancies }} open discrepancies</p></div>
        <button class="close-button" type="button" :disabled="actionLoading || manifest.status !== 'Open' || counts.pending > 0 || counts.openDiscrepancies > 0" @click="$emit('close')"><CheckCircle2 :size="16" />Close manifest</button> -->
        <section class="reconciliation-panel">

      <div class="reconciliation-header">
        <div>
          <h3>Manifest reconciliation</h3>
          <p>Review all specimen statuses before closing this manifest.</p>
        </div>

        <span
          v-if="manifest.status === 'Closed'"
          class="reconciliation-status complete"
        >
          Closed
        </span>

        <span
          v-else-if="
            counts.pending === 0 &&
            counts.openDiscrepancies === 0
          "
          class="reconciliation-status complete"
        >
          Ready to close
        </span>

        <span v-else class="reconciliation-status pending">
          Action required
        </span>
      </div>

      <div class="reconciliation-grid">

        <div class="reconciliation-item">
          <span>Expected</span>
          <strong>{{ counts.expected }}</strong>
        </div>

        <div class="reconciliation-item">
          <span>Received</span>
          <strong>{{ counts.received }}</strong>
        </div>

        <div class="reconciliation-item">
          <span>Pending</span>
          <strong>{{ counts.pending }}</strong>
        </div>

        <div class="reconciliation-item">
          <span>Flagged</span>
          <strong>{{ counts.flagged }}</strong>
        </div>

        <div class="reconciliation-item">
          <span>Open discrepancies</span>
          <strong>{{ counts.openDiscrepancies }}</strong>
        </div>

      </div>

      <div
        v-if="manifest.status === 'Closed'"
        class="reconciliation-message success-message"
      >
        All reconciliation requirements were satisfied.
        This manifest is closed.
      </div>

      <div
        v-else-if="
          counts.pending === 0 &&
          counts.openDiscrepancies === 0
        "
        class="reconciliation-message success-message"
      >
        All specimens have been accounted for and all discrepancies
        have been resolved. You can close this manifest.
      </div>

      <div v-else class="reconciliation-message warning-message">

        <template v-if="counts.pending > 0">
          {{ counts.pending }} specimen(s) are still pending.
        </template>

        <template v-if="counts.pending > 0 && counts.openDiscrepancies > 0">
          And
        </template>

        <template v-if="counts.openDiscrepancies > 0">
          {{ counts.openDiscrepancies }} discrepancy(s) still need resolution.
        </template>

        Complete these actions before closing.

      </div>

      <div class="footer-actions">

        <div>
          <strong>Close manifest</strong>
          <p>Closing prevents further specimen check-in actions.</p>
        </div>

        <button
          class="close-button"
          :disabled="
            actionLoading ||
            manifest.status !== 'Open' ||
            counts.pending > 0 ||
            counts.openDiscrepancies > 0
          "
          @click="emit('close')"
        >
          <CheckCircle2 :size="16" />
          Close manifest
        </button>

      </div>

    </section>
      </footer>
    </template>
  </section>
</template>
