(() => {
  const $ = (selector) => document.querySelector(selector);
  const toast = $('#toast');
  let toastTimer;
  const showToast = (message) => {
    toast.textContent = message;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 2300);
  };

  $('#start-button').addEventListener('click', () => {
    $('#running-tag').classList.remove('stopped');
    $('#running-tag').innerHTML = '<i></i> Driver running';
    $('#start-button').textContent = 'Running';
    $('#start-button').disabled = true;
    $('#start-button').style.opacity = '.68';
    showToast('Driver started in preview mode');
  });
  $('#stop-button').addEventListener('click', () => {
    $('#running-tag').classList.add('stopped');
    $('#running-tag').innerHTML = '<i></i> Driver stopped';
    $('#start-button').innerHTML = '<span class="play-icon">▶</span> Start driver';
    $('#start-button').disabled = false;
    $('#start-button').style.opacity = '1';
    showToast('Driver stopped');
  });
  $('#detect-button').addEventListener('click', () => showToast('VEIKK S640 detected · VID 2FEB / PID 0001'));
  const changed = () => {
    $('#saved-state').innerHTML = '<span class="saved-check">●</span> Unsaved changes';
  };
  document.querySelectorAll('.area-fields input, .display-fields input, .binding-row select, .switch').forEach((control) => {
    control.addEventListener('input', changed);
    control.addEventListener('change', changed);
  });
  $('#apply-button').addEventListener('click', () => {
    $('#saved-state').innerHTML = '<span class="saved-check">✓</span> Applied · not saved';
    showToast('Mapping settings applied');
  });
  $('#save-button').addEventListener('click', () => {
    $('#saved-state').innerHTML = '<span class="saved-check">✓</span> All changes saved';
    showToast('Settings applied and saved');
  });
  $('#reset-button').addEventListener('click', () => {
    document.querySelectorAll('.display-fields input').forEach((input, i) => input.value = i === 0 ? '1920' : '1080');
    document.querySelectorAll('.area-fields input').forEach((input, i) => input.value = ['152.4', '101.6', '76.2', '50.8', '0', '0'][i]);
    $('#saved-state').innerHTML = '<span class="saved-check">●</span> Unsaved changes';
    showToast('Display mapping reset');
  });
  $('#profile-button').addEventListener('click', () => showToast('VEIKK S640 profile selected'));
  $('#settings-button').addEventListener('click', () => showToast('Settings will open here'));

  // A deterministic preview signal keeps the mockup useful without a hardware bridge.
  let tick = 0;
  setInterval(() => {
    tick += 1;
    const rate = 836 + Math.round(Math.sin(tick / 4) * 18) + (tick % 5);
    $('#rate-reading').textContent = rate;
    $('#core-reading').textContent = (0.18 + ((tick % 4) * 0.01)).toFixed(2);
    $('#memory-reading').textContent = (14.7 + ((tick % 4) * 0.03)).toFixed(1);
  }, 1000);
})();
