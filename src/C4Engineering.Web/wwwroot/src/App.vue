<template>
  <div id="app">
    <!-- Navigation Header -->
    <BNavbar type="dark" variant="dark" fixed="top">
      <BContainer>
        <BNavbarBrand :to="{ name: 'Dashboard' }" class="fw-bold">
          <i class="bi bi-diagram-3 me-2"></i>
          C4Engineering
        </BNavbarBrand>

        <BNavbarToggle target="nav-collapse"></BNavbarToggle>

        <BCollapse id="nav-collapse" is-nav>
          <BNavbarNav class="ms-auto">
            <BNavItem :to="{ name: 'Dashboard' }">
              <i class="bi bi-house me-1"></i>
              Dashboard
            </BNavItem>
            <BNavItem :to="{ name: 'ServiceCatalog' }">
              <i class="bi bi-box-seam me-1"></i>
              Service Catalog
            </BNavItem>
            <BNavItem :to="{ name: 'Architecture' }">
              <i class="bi bi-diagram-3 me-1"></i>
              Architecture
            </BNavItem>
            <BNavItem :to="{ name: 'Pipelines' }">
              <i class="bi bi-gear me-1"></i>
              Pipelines
            </BNavItem>
            <BNavItem :to="{ name: 'Deployments' }">
              <i class="bi bi-play-circle me-1"></i>
              Deployments
            </BNavItem>
          </BNavbarNav>

          <!-- User Actions -->
          <BNavbarNav class="ms-3">
            <BNavItemDropdown text="User" right>
              <BDropdownItem href="#">
                <i class="bi bi-person me-1"></i>
                Profile
              </BDropdownItem>
              <BDropdownItem href="#">
                <i class="bi bi-gear me-1"></i>
                Settings
              </BDropdownItem>
              <BDropdownDivider></BDropdownDivider>
              <BDropdownItem href="#">
                <i class="bi bi-box-arrow-right me-1"></i>
                Logout
              </BDropdownItem>
            </BNavItemDropdown>
          </BNavbarNav>
        </BCollapse>
      </BContainer>
    </BNavbar>

    <!-- Main Content -->
    <main class="main-content">
      <BContainer fluid>
        <!-- Breadcrumb -->
        <BBreadcrumb v-if="breadcrumbs.length > 0" class="mt-3">
          <BBreadcrumbItem
            v-for="(crumb, index) in breadcrumbs"
            :key="index"
            :to="crumb.to"
            :active="index === breadcrumbs.length - 1"
          >
            {{ crumb.text }}
          </BBreadcrumbItem>
        </BBreadcrumb>

        <!-- Page Content -->
        <RouterView />
      </BContainer>
    </main>

    <!-- Footer -->
    <footer class="footer mt-auto py-3 bg-light border-top">
      <BContainer>
        <div class="d-flex justify-content-between align-items-center">
          <span class="text-muted">
            &copy; 2025 C4Engineering Platform - Platform Engineering MVP
          </span>
          <div>
            <BButton variant="outline-secondary" size="sm" href="/api-docs" target="_blank">
              <i class="bi bi-code me-1"></i>
              API Docs
            </BButton>
          </div>
        </div>
      </BContainer>
    </footer>

    <!-- Global Loading Overlay -->
    <div v-if="isLoading" class="global-loading-overlay">
      <div class="d-flex justify-content-center align-items-center h-100">
        <BSpinner variant="primary" style="width: 3rem; height: 3rem;" class="me-3"></BSpinner>
        <span class="fs-5">Loading...</span>
      </div>
    </div>

    <!-- Toast Container -->
    <div class="toast-container position-fixed bottom-0 end-0 p-3">
      <BToast
        v-for="toast in toasts"
        :key="toast.id"
        :variant="toast.variant"
        :title="toast.title"
        :auto-hide-delay="5000"
        visible
        @hidden="removeToast(toast.id)"
      >
        {{ toast.message }}
      </BToast>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from './stores/app'

const route = useRoute()
const appStore = useAppStore()

const isLoading = computed(() => appStore.isLoading)
const toasts = computed(() => appStore.toasts)
const breadcrumbs = computed(() => appStore.breadcrumbs)

const removeToast = (id) => {
  appStore.removeToast(id)
}

onMounted(() => {
  // Initialize the application
  appStore.initialize()
})
</script>

<style scoped>
.main-content {
  margin-top: 56px; /* Height of fixed navbar */
  min-height: calc(100vh - 56px - 60px); /* Full height minus navbar and footer */
}

.footer {
  margin-top: auto;
}

.global-loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(255, 255, 255, 0.9);
  z-index: 9999;
}

#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}
</style>