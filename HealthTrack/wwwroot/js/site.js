// HealthTrack — Client-Side Scripting
// Supports accessible password visibility toggle & real-time password requirement guidance

document.addEventListener('DOMContentLoaded', function () {
    initPasswordToggles();
    initPasswordGuidance();
});

/**
 * Initializes accessible password show/hide functionality for password fields.
 */
function initPasswordToggles() {
    const toggleButtons = document.querySelectorAll('.ht-pwd-toggle');

    toggleButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const inputBox = this.closest('.ht-input-box');
            if (!inputBox) return;

            const passwordInput = inputBox.querySelector('input');
            if (!passwordInput) return;

            const eyeOpenIcon = this.querySelector('.icon-eye-open');
            const eyeClosedIcon = this.querySelector('.icon-eye-closed');

            if (passwordInput.type === 'password') {
                passwordInput.type = 'text';
                this.setAttribute('aria-label', 'Hide password');
                if (eyeOpenIcon) eyeOpenIcon.classList.add('d-none');
                if (eyeClosedIcon) eyeClosedIcon.classList.remove('d-none');
            } else {
                passwordInput.type = 'password';
                this.setAttribute('aria-label', 'Show password');
                if (eyeOpenIcon) eyeOpenIcon.classList.remove('d-none');
                if (eyeClosedIcon) eyeClosedIcon.classList.add('d-none');
            }

            passwordInput.focus();
        });
    });
}

/**
 * Live password guidance indicator for the registration page.
 * Evaluates minimum 8 characters and alphanumeric combination.
 */
function initPasswordGuidance() {
    const passwordInput = document.getElementById('register-password');
    const ruleLength = document.getElementById('rule-length');
    const ruleCombo = document.getElementById('rule-combo');

    if (!passwordInput || !ruleLength || !ruleCombo) return;

    passwordInput.addEventListener('input', function () {
        const val = this.value;

        // Check 1: 8+ characters
        const hasLength = val.length >= 8;
        toggleRuleState(ruleLength, hasLength);

        // Check 2: Letter and number
        const hasLetter = /[a-zA-Z]/.test(val);
        const hasNumber = /[0-9]/.test(val);
        toggleRuleState(ruleCombo, hasLetter && hasNumber);
    });
}

function toggleRuleState(element, isValid) {
    if (isValid) {
        element.classList.add('valid');
        const icon = element.querySelector('.ht-rule-dot');
        if (icon) {
            icon.innerHTML = '<polyline points="20 6 9 17 4 12" stroke-width="2.5"></polyline>';
        }
    } else {
        element.classList.remove('valid');
        const icon = element.querySelector('.ht-rule-dot');
        if (icon) {
            icon.innerHTML = '<circle cx="12" cy="12" r="10"></circle>';
        }
    }
}
