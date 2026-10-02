// HomeCare - Client-side interactivity
document.addEventListener('DOMContentLoaded', function () {
    // 1. Mobile Sidebar Toggle
    const sidebarToggleBtn = document.getElementById('sidebarToggleBtn');
    const appSidebar = document.querySelector('.app-sidebar');
    const sidebarOverlay = document.querySelector('.sidebar-overlay');

    if (sidebarToggleBtn && appSidebar) {
        sidebarToggleBtn.addEventListener('click', function () {
            appSidebar.classList.toggle('show');
            if (sidebarOverlay) sidebarOverlay.classList.toggle('show');
        });
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function () {
            appSidebar.classList.remove('show');
            sidebarOverlay.classList.remove('show');
        });
    }

    // 2. Auto-dismiss alerts after 5 seconds
    const autoAlerts = document.querySelectorAll('.alert-dismissible');
    autoAlerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 6000);
    });

    // 3. Notification Bell Polling & Dropdown Loader
    const notifBell = document.getElementById('notificationBellBtn');
    const notifBadge = document.getElementById('notificationBadge');
    const notifDropdownList = document.getElementById('notificationDropdownList');

    function updateNotifications() {
        if (!notifBell) return;

        fetch('/Notification/GetUnreadCount')
            .then(res => res.json())
            .then(data => {
                if (notifBadge) {
                    if (data.count > 0) {
                        notifBadge.textContent = data.count > 99 ? '99+' : data.count;
                        notifBadge.style.display = 'inline-block';
                    } else {
                        notifBadge.style.display = 'none';
                    }
                }
            })
            .catch(() => {});
    }

    if (notifBell) {
        updateNotifications();

        notifBell.addEventListener('show.bs.dropdown', function () {
            if (!notifDropdownList) return;
            notifDropdownList.innerHTML = '<li class="p-3 text-center text-muted"><div class="spinner-border spinner-border-sm" role="status"></div> Loading alerts...</li>';

            fetch('/Notification/GetRecent')
                .then(res => res.json())
                .then(items => {
                    if (items.length === 0) {
                        notifDropdownList.innerHTML = '<li class="p-3 text-center text-muted"><i class="bi bi-bell-slash d-block fs-3 mb-1"></i> No unread notifications</li>';
                        return;
                    }

                    let html = '';
                    items.forEach(item => {
                        const unreadClass = item.isRead ? '' : 'unread';
                        const link = item.linkUrl || '/Notification';
                        html += `
                            <li>
                                <a href="${link}" class="notification-item ${unreadClass}">
                                    <div class="d-flex justify-content-between align-items-center mb-1">
                                        <strong class="text-primary small">${item.title}</strong>
                                        <small class="text-muted">${item.timeAgo}</small>
                                    </div>
                                    <p class="small text-secondary mb-0">${item.message}</p>
                                </a>
                            </li>
                        `;
                    });

                    html += `
                        <li class="p-2 text-center border-top bg-light">
                            <a href="/Notification" class="text-decoration-none small fw-bold">View All Notifications &rarr;</a>
                        </li>
                    `;
                    notifDropdownList.innerHTML = html;
                })
                .catch(() => {
                    notifDropdownList.innerHTML = '<li class="p-3 text-center text-danger">Failed to load alerts</li>';
                });
        });
    }

    // 4. Generic Confirmation Modals
    const confirmDeleteModal = document.getElementById('confirmDeleteModal');
    if (confirmDeleteModal) {
        confirmDeleteModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const actionUrl = button.getAttribute('data-action');
            const itemName = button.getAttribute('data-item-name') || 'this item';
            const itemType = button.getAttribute('data-item-type') || 'Record';

            const modalItemText = confirmDeleteModal.querySelector('#deleteModalItemName');
            const modalItemType = confirmDeleteModal.querySelector('#deleteModalItemType');
            const modalForm = confirmDeleteModal.querySelector('#deleteModalForm');

            if (modalItemText) modalItemText.textContent = itemName;
            if (modalItemType) modalItemType.textContent = itemType;
            if (modalForm && actionUrl) modalForm.action = actionUrl;
        });
    }
});
