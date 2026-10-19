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
  const areaInputs = [...document.querySelectorAll('.area-fields input')];
  const areaPercentInputs = [$('#width-percent'), $('#height-percent')];
  const areaShape = $('#tablet-shape');
  const areaStage = $('.tablet-visual');
  let areaOrientation = 'landscape';
  const baseArea = { landscape: [152.4, 101.6], portrait: [76.2, 101.6] };
  const updateAreaShape = (orientation, applyCoverage = false, syncCoverage = false) => {
    if (orientation) areaOrientation = orientation;
    if (applyCoverage) {
      const [baseWidth, baseHeight] = baseArea[areaOrientation];
      areaInputs[0].value = (baseWidth * Number(areaPercentInputs[0].value) / 100).toFixed(1);
      areaInputs[1].value = (baseHeight * Number(areaPercentInputs[1].value) / 100).toFixed(1);
    }
    const width = Number(areaInputs[0].value);
    const height = Number(areaInputs[1].value);
    if (!(width > 0 && height > 0)) return;
    const ratio = width / height;
    const selected = orientation || (ratio < 1 ? 'portrait' : 'landscape');
    areaOrientation = selected;
    if (syncCoverage) {
      const [baseWidth, baseHeight] = baseArea[areaOrientation];
      areaPercentInputs[0].value = String(Math.max(1, Math.min(100, Math.round(width / baseWidth * 100))));
      areaPercentInputs[1].value = String(Math.max(1, Math.min(100, Math.round(height / baseHeight * 100))));
    }
    $('#width-percent-value').value = `${areaPercentInputs[0].value}%`;
    $('#width-percent-value').textContent = `${areaPercentInputs[0].value}%`;
    $('#height-percent-value').value = `${areaPercentInputs[1].value}%`;
    $('#height-percent-value').textContent = `${areaPercentInputs[1].value}%`;
    const maxWidth = areaStage.clientWidth * 0.78;
    const maxHeight = areaStage.clientHeight * 0.84;
    let previewWidth = maxWidth;
    let previewHeight = previewWidth / ratio;
    if (previewHeight > maxHeight) {
      previewHeight = maxHeight;
      previewWidth = previewHeight * ratio;
    }
    areaShape.style.width = `${previewWidth}px`;
    areaShape.style.height = `${previewHeight}px`;
    areaShape.setAttribute('aria-label', `${selected} tablet area preview`);
    $('#shape-dimensions').textContent = `${areaInputs[0].value} × ${areaInputs[1].value} mm`;
    document.querySelectorAll('.orientation-choice').forEach((button) => {
      const active = button.dataset.orientation === selected;
      button.classList.toggle('selected', active);
      button.setAttribute('aria-pressed', String(active));
    });
  };
  document.querySelectorAll('.orientation-choice').forEach((button) => {
    button.addEventListener('click', () => {
      updateAreaShape(button.dataset.orientation, true);
      changed();
    });
  });
  areaPercentInputs.forEach((input) => input.addEventListener('input', () => updateAreaShape(areaOrientation, true)));
  areaInputs.slice(0, 2).forEach((input) => input.addEventListener('input', () => updateAreaShape(undefined, false, true)));
  updateAreaShape('landscape', true);
  new ResizeObserver(() => updateAreaShape()).observe(areaStage);
  document.querySelectorAll('.area-fields input, .percentage-control input, .display-fields input, .binding-row select, .switch').forEach((control) => {
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
    areaInputs.forEach((input, i) => input.value = ['76.2', '50.8', '76.2', '50.8', '0', '0'][i]);
    areaPercentInputs.forEach((input) => input.value = '50');
    updateAreaShape('landscape', true);
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
