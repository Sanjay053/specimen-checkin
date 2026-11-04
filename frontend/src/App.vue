<script setup>
import { computed, onMounted, ref } from 'vue';
import { AlertTriangle, X } from 'lucide-vue-next';
import AppHeader from './components/AppHeader.vue';
import ManifestList from './components/ManifestList.vue';
import ManifestDetails from './components/ManifestDetails.vue';
import {
  closeManifest as closeManifestApi,
  flagSpecimen as flagSpecimenApi,
  getApiError,
  getManifestDetails,
  getManifests,
  receiveSpecimen as receiveSpecimenApi,
  resolveDiscrepancy as resolveDiscrepancyApi,
} from './services/api';

const manifests = ref([]);
const selectedManifest = ref(null);
const loading = ref(false);
const detailsLoading = ref(false);
const actionLoading = ref(false);
const error = ref('');
const notice = ref('');

const specimens = computed(() => selectedManifest.value?.specimens ?? []);
const discrepancies = computed(
  () => selectedManifest.value?.discrepancyDetails ?? []
);


const counts = computed(() => {
  const list = specimens.value;

  const received = list.filter(
    (item) => String(item.status).toLowerCase() === "received"
  ).length;

  const pending = list.filter(
    (item) => String(item.status).toLowerCase() === "pending"
  ).length;

  const flagged = list.filter(
    (item) => String(item.status).toLowerCase() === "flagged"
  ).length;

  const openDiscrepancies = discrepancies.value.filter(
    (item) => String(item.status).toLowerCase() === "open"
  ).length;

  return {
    expected: Number(
      selectedManifest.value?.expected ??
      selectedManifest.value?.expectedCount ??
      list.length
    ),
    received,
    pending,
    flagged,
    discrepancies: discrepancies.value.length,
    openDiscrepancies,
  };
});

function normalizeDetail(payload) {
  const manifest = payload?.manifest ?? payload ?? {};

  return {
    ...manifest,
    specimens: Array.isArray(payload?.specimens)
      ? payload.specimens
      : Array.isArray(manifest?.specimens)
        ? manifest.specimens
        : [],

    discrepancyDetails: Array.isArray(payload?.discrepancyDetails)
      ? payload.discrepancyDetails
      : Array.isArray(manifest?.discrepancyDetails)
        ? manifest.discrepancyDetails
        : [],
  };
}

function getError(err) {
  return (
    err.response?.data?.error ||
    err.response?.data?.message ||
    err.response?.data?.title ||
    err.message ||
    "Something went wrong."
  );
}

async function selectManifest(id) {
  detailsLoading.value = true;
  error.value = "";

  try {
    const response = await getManifestDetails(id);
    const data = response.data;

    selectedManifest.value = {
      ...data,
      specimens: Array.isArray(data.specimens)
        ? data.specimens
        : [],
      discrepancyDetails: Array.isArray(data.discrepancyDetails)
        ? data.discrepancyDetails
        : [],
    };
  } catch (err) {
    error.value = getError(err);
  } finally {
    detailsLoading.value = false;
  }
}

async function loadManifests() {
  loading.value = true;
  error.value = '';
  try {
    const response = await getManifests();
    manifests.value = Array.isArray(response.data) ? response.data : response.data?.items ?? [];
    if (!manifests.value.length) {
      selectedManifest.value = null;
      return;
    }
    const selectedId = selectedManifest.value?.id;
    const next = manifests.value.find((item) => String(item.id) === String(selectedId)) ?? manifests.value[0];
    await selectManifest(next.id);
  } catch (err) {
    error.value = getApiError(err);
  } finally {
    loading.value = false;
  }
}

async function refreshData() {
  await loadManifests();
}

async function runAction(action, successMessage) {
  const manifestId = selectedManifest.value?.id;
  if (!manifestId) return;
  actionLoading.value = true;
  error.value = '';
  notice.value = '';
  try {
    await action(manifestId);
    notice.value = successMessage;
    await loadManifests();
  } catch (err) {
    error.value = getApiError(err);
  } finally {
    actionLoading.value = false;
  }
}

async function receiveSpecimen(specimenId) {
  await runAction((manifestId) => receiveSpecimenApi(manifestId, specimenId), 'Specimen marked as received.');
}

async function flagSpecimen(specimenId) {
  if (!window.confirm('Flag this specimen as missing and create a discrepancy?')) return;
  await runAction((manifestId) => flagSpecimenApi(manifestId, specimenId), 'Specimen flagged and discrepancy recorded.');
}

async function resolveDiscrepancy(discrepancyId) {
  if (!window.confirm('Mark this discrepancy as resolved?')) return;
  await runAction((manifestId) => resolveDiscrepancyApi(manifestId, discrepancyId), 'Discrepancy resolved.');
}

async function closeSelectedManifest() {
  if (!window.confirm('Close this manifest? This action may not be reversible.')) return;
  await runAction((manifestId) => closeManifestApi(manifestId), 'Manifest closed successfully.');
}

onMounted(loadManifests);
</script>

<template>
  <div class="app-shell">
    <AppHeader :refreshing="loading" @refresh="refreshData" />
    <main class="dashboard">
      <div class="page-heading">
        <div><span class="eyebrow">LAB OPERATIONS / SPECIMEN INTAKE</span><h1>Specimen Check-In</h1><p>Track incoming specimens and reconcile shipment manifests.</p></div>
        <span class="connection-status"><span class="online-dot" />API connected</span>
      </div>

      <div v-if="error" class="alert-banner error-banner" role="alert"><AlertTriangle :size="17" /><span>{{ error }}</span><button type="button" aria-label="Dismiss error" @click="error = ''"><X :size="16" /></button></div>
      <div v-if="notice" class="alert-banner success-banner" role="status"><span>{{ notice }}</span><button type="button" aria-label="Dismiss notification" @click="notice = ''"><X :size="16" /></button></div>

      <div class="workspace">
        <ManifestList :manifests="manifests" :selected-id="selectedManifest?.id" :loading="loading" @select="selectManifest" />
        <ManifestDetails
          :manifest="selectedManifest"
          :specimens="specimens"
          :discrepancies="discrepancies"
          :counts="counts"
          :loading="detailsLoading"
          :action-loading="actionLoading"
          @refresh="refreshData"
          @receive="receiveSpecimen"
          @flag="flagSpecimen"
          @resolve="resolveDiscrepancy"
          @close="closeSelectedManifest"
        />
      </div>
    </main>
  </div>
</template>
