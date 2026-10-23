/**
 * HealthTrack Admin Portal — Scoped JavaScript
 * Handles user searching, role filtering, modal states, and backend handover notices.
 */

document.addEventListener('DOMContentLoaded', function () {
    // -------------------------------------------------------------------------
    // 1. Mobile Sidebar Navigation Toggle
    // -------------------------------------------------------------------------
    var openSidebarBtn = document.getElementById('openSidebarBtn');
    var closeSidebarBtn = document.getElementById('closeSidebarBtn');
    var portalSidebar = document.getElementById('portalSidebar');
    var sidebarBackdrop = document.getElementById('sidebarBackdrop');

    function openSidebar() {
        if (portalSidebar) portalSidebar.classList.add('show');
        if (sidebarBackdrop) sidebarBackdrop.classList.add('show');
    }

    function closeSidebar() {
        if (portalSidebar) portalSidebar.classList.remove('show');
        if (sidebarBackdrop) sidebarBackdrop.classList.remove('show');
    }

    if (openSidebarBtn) openSidebarBtn.addEventListener('click', openSidebar);
    if (closeSidebarBtn) closeSidebarBtn.addEventListener('click', closeSidebar);
    if (sidebarBackdrop) sidebarBackdrop.addEventListener('click', closeSidebar);

    // -------------------------------------------------------------------------
    // 2. User Management: Search & Filter Table
    // -------------------------------------------------------------------------
    var userSearchInput = document.getElementById('userSearchInput');
    var filterPills = document.querySelectorAll('.ht-filter-pill');
    var userRows = document.querySelectorAll('.ht-user-row');
    var emptyStateRow = document.getElementById('userEmptyState');
    var visibleUsersCount = document.getElementById('visibleUsersCount');

    var currentRoleFilter = 'all';

    function filterUserTable() {
        if (!userRows || userRows.length === 0) return;

        var searchTerm = userSearchInput ? userSearchInput.value.trim().toLowerCase() : '';
        var matchCount = 0;

        userRows.forEach(function (row) {
            var email = (row.getAttribute('data-email') || '').toLowerCase();
            var role = (row.getAttribute('data-role') || '').toLowerCase();
            var id = (row.getAttribute('data-id') || '').toLowerCase();

            var matchesSearch = !searchTerm || email.indexOf(searchTerm) !== -1 || id.indexOf(searchTerm) !== -1 || role.indexOf(searchTerm) !== -1;
            var matchesRole = currentRoleFilter === 'all' || role === currentRoleFilter;

            if (matchesSearch && matchesRole) {
                row.style.display = '';
                matchCount++;
            } else {
                row.style.display = 'none';
            }
        });

        if (emptyStateRow) {
            emptyStateRow.style.display = matchCount === 0 ? '' : 'none';
        }

        if (visibleUsersCount) {
            visibleUsersCount.textContent = matchCount.toString();
        }
    }

    if (userSearchInput) {
        userSearchInput.addEventListener('input', filterUserTable);
    }

    filterPills.forEach(function (pill) {
        pill.addEventListener('click', function () {
            filterPills.forEach(function (p) { p.classList.remove('active'); });
            pill.classList.add('active');
            currentRoleFilter = pill.getAttribute('data-role-filter') || 'all';
            filterUserTable();
        });
    });

    // -------------------------------------------------------------------------
    // 3. User Role Management Modal & Safe Handover Notice
    // -------------------------------------------------------------------------
    var roleModal = document.getElementById('htRoleModal');
    var roleModalClose = document.getElementById('roleModalClose');
    var roleModalCancel = document.getElementById('roleModalCancel');
    var manageRoleButtons = document.querySelectorAll('.ht-manage-role-btn');

    var modalUserEmail = document.getElementById('modalUserEmail');
    var modalUserId = document.getElementById('modalUserId');
    var modalCurrentRole = document.getElementById('modalCurrentRole');
    var modalRoleSelect = document.getElementById('modalRoleSelect');
    var modalSelfDemoteNotice = document.getElementById('modalSelfDemoteNotice');
    var modalSubmitBtn = document.getElementById('modalSubmitRoleBtn');

    function openRoleModal(button) {
        if (!roleModal) return;

        var userId = button.getAttribute('data-user-id') || '';
        var userEmail = button.getAttribute('data-user-email') || '';
        var userRole = button.getAttribute('data-user-role') || 'Client';

        if (modalUserEmail) modalUserEmail.textContent = userEmail;
        if (modalUserId) modalUserId.value = userId;
        if (modalCurrentRole) modalCurrentRole.textContent = userRole;
        if (modalRoleSelect) modalRoleSelect.value = userRole;

        // Check for Admin self-demotion risk warning
        var isCurrentAdmin = userEmail.toLowerCase() === 'admin@healthtrack.com';
        if (modalSelfDemoteNotice) {
            modalSelfDemoteNotice.style.display = isCurrentAdmin ? 'block' : 'none';
        }

        roleModal.classList.add('show');
    }

    function closeRoleModal() {
        if (roleModal) roleModal.classList.remove('show');
    }

    manageRoleButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            openRoleModal(btn);
        });
    });

    if (roleModalClose) roleModalClose.addEventListener('click', closeRoleModal);
    if (roleModalCancel) roleModalCancel.addEventListener('click', closeRoleModal);

    // Prevent submitting to unintegrated endpoint & clarify contract
    if (modalSubmitBtn) {
        modalSubmitBtn.addEventListener('click', function (e) {
            e.preventDefault();
            alert('Backend Handover Notice:\n\nRole modification requires backend endpoint implementation: POST /Admin/UpdateUserRole.\n\nRequired steps for backend teammate:\n1. UserManager<User>.AddToRoleAsync() & RemoveFromRoleAsync()\n2. Sync custom User.role property\n3. Synchronize Client/Trainer entity profile records\n4. Server-side CSRF validation & self-demotion prevention.');
        });
    }

    // -------------------------------------------------------------------------
    // 4. Trainer Assignments Preview Modal & Interactions
    // -------------------------------------------------------------------------
    var assignModal = document.getElementById('htAssignModal');
    var assignModalClose = document.getElementById('assignModalClose');
    var assignModalCancel = document.getElementById('assignModalCancel');
    var assignButtons = document.querySelectorAll('.ht-assign-trainer-btn');

    var assignClientName = document.getElementById('assignClientName');
    var assignClientId = document.getElementById('assignClientId');
    var assignCurrentTrainer = document.getElementById('assignCurrentTrainer');
    var assignTrainerSelect = document.getElementById('assignTrainerSelect');
    var assignSubmitBtn = document.getElementById('assignSubmitBtn');

    function openAssignModal(button) {
        if (!assignModal) return;

        var clientId = button.getAttribute('data-client-id') || '';
        var clientName = button.getAttribute('data-client-name') || '';
        var currentTrainer = button.getAttribute('data-current-trainer') || 'Unassigned';

        if (assignClientName) assignClientName.textContent = clientName;
        if (assignClientId) assignClientId.value = clientId;
        if (assignCurrentTrainer) assignCurrentTrainer.textContent = currentTrainer;

        assignModal.classList.add('show');
    }

    function closeAssignModal() {
        if (assignModal) assignModal.classList.remove('show');
    }

    assignButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            openAssignModal(btn);
        });
    });

    if (assignModalClose) assignModalClose.addEventListener('click', closeAssignModal);
    if (assignModalCancel) assignModalCancel.addEventListener('click', closeAssignModal);

    if (assignSubmitBtn) {
        assignSubmitBtn.addEventListener('click', function (e) {
            e.preventDefault();
            alert('Database Schema Handover Notice:\n\nThe current schema does not have a direct Client-to-Trainer relationship (Client.trainerId foreign key).\n\nRecommended implementation for backend developer:\n1. Add nullable int? trainerId to Client entity\n2. Add EF Core migration for the foreign key\n3. Implement POST /Admin/AssignTrainer endpoint.');
            closeAssignModal();
        });
    }

    // Close modals on backdrop click
    window.addEventListener('click', function (e) {
        if (e.target === roleModal) closeRoleModal();
        if (e.target === assignModal) closeAssignModal();
    });
});
