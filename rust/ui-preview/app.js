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
  const areaInputs = ['#area-width', '#area-height', '#x-offset', '#y-offset', '#rotation', '#tip-pressure'].map((id) => $(id));
  const aspectSlider = $('#aspect-ratio');
  const aspectLock = $('#aspect-lock');
  const areaShape = $('#tablet-shape');
  const areaStage = $('.tablet-visual');
  const tablet = { width: 152.4, height: 101.6 };
  const displayFields = [...document.querySelectorAll('.display-fields input')];
  const display = { width: Number(displayFields[0].value), height: Number(displayFields[1].value) };
  const clamp = (value, min, max) => Math.min(max, Math.max(min, value));
  const updateAreaShape = () => {
    const width = Number(areaInputs[0].value);
    const height = Number(areaInputs[1].value);
    if (!(width > 0 && height > 0)) return;
    if (aspectLock.checked) {
      aspectSlider.value = (width / height).toFixed(2);
    }
    const maxWidth = areaStage.clientWidth * 0.78;
    const maxHeight = areaStage.clientHeight * 0.84;
    let previewWidth = maxWidth * width / tablet.width;
    let previewHeight = maxHeight * height / tablet.height;
    const radians = Number(areaInputs[4].value) * Math.PI / 180;
    const rotatedWidth = Math.abs(previewWidth * Math.cos(radians)) + Math.abs(previewHeight * Math.sin(radians));
    const rotatedHeight = Math.abs(previewHeight * Math.cos(radians)) + Math.abs(previewWidth * Math.sin(radians));
    const fit = Math.min(1, maxWidth / rotatedWidth, maxHeight / rotatedHeight);
    previewWidth *= fit;
    previewHeight *= fit;
    if (previewHeight > maxHeight) {
      previewHeight = maxHeight;
      previewWidth = previewHeight * width / height;
    }
    areaShape.style.width = `${previewWidth}px`;
    areaShape.style.height = `${previewHeight}px`;
    areaShape.style.transform = `translate(${(Number(areaInputs[2].value) / tablet.width - .5) * maxWidth * .8}px, ${(Number(areaInputs[3].value) / tablet.height - .5) * maxHeight * .8}px) rotate(${areaInputs[4].value}deg)`;
    areaShape.setAttribute('aria-label', `${width.toFixed(1)} by ${height.toFixed(1)} millimeter mapped area, rotated ${areaInputs[4].value} degrees`);
    $('#shape-dimensions').textContent = `${width.toFixed(1)} × ${height.toFixed(1)} mm`;
    $('#center-label').textContent = `${Number(areaInputs[2].value).toFixed(1)}, ${Number(areaInputs[3].value).toFixed(1)} mm`;
    $('#area-ratio-label').textContent = `${(width / height).toFixed(2)}`;
    $('#display-ratio-action').textContent = `${(display.width / display.height).toFixed(2)}`;
    $('#x-value').textContent = `${Number(areaInputs[2].value).toFixed(1)} mm`;
    $('#y-value').textContent = `${Number(areaInputs[3].value).toFixed(1)} mm`;
    $('#width-value').textContent = `${width.toFixed(1)} mm`;
    $('#height-value').textContent = `${height.toFixed(1)} mm`;
    $('#rotation-value').textContent = `${Number(areaInputs[4].value)}°`;
    $('#aspect-value').textContent = `${Number(aspectSlider.value).toFixed(2)}`;
    $('#pressure-value').textContent = `${Number(areaInputs[5].value)}%`;
    document.querySelectorAll('.rotation-presets button').forEach((button) => {
      button.classList.toggle('selected', Number(button.dataset.rotation) === Number(areaInputs[4].value));
    });
  };
  const setAreaSize = (changedInput) => {
    const ratio = Number(aspectSlider.value);
    if (aspectLock.checked && ratio > 0) {
      if (changedInput === areaInputs[1]) areaInputs[0].value = clamp(Number(areaInputs[1].value) * ratio, 1, tablet.width).toFixed(1);
      else areaInputs[1].value = clamp(Number(areaInputs[0].value) / ratio, 1, tablet.height).toFixed(1);
    } else {
      aspectSlider.value = (Number(areaInputs[0].value) / Number(areaInputs[1].value)).toFixed(2);
    }
    updateAreaShape();
  };
  areaInputs.forEach((input) => input.addEventListener('input', () => {
    if (input === areaInputs[0] || input === areaInputs[1]) setAreaSize(input);
    else updateAreaShape();
    changed();
  }));
  aspectSlider.addEventListener('input', () => {
    if (aspectLock.checked) areaInputs[1].value = clamp(Number(areaInputs[0].value) / Number(aspectSlider.value), 1, tablet.height).toFixed(1);
    updateAreaShape();
    changed();
  });
  aspectLock.addEventListener('change', () => {
    if (aspectLock.checked) setAreaSize(areaInputs[0]);
    changed();
  });
  $('#rotation').addEventListener('input', updateAreaShape);
  document.querySelectorAll('.rotation-presets button').forEach((button) => button.addEventListener('click', () => {
    areaInputs[4].value = button.dataset.rotation;
    updateAreaShape();
    changed();
  }));
  $('#full-area-button').addEventListener('click', () => {
    areaInputs[0].value = tablet.width;
    areaInputs[1].value = tablet.height;
    areaInputs[2].value = tablet.width / 2;
    areaInputs[3].value = tablet.height / 2;
    areaInputs[4].value = '0';
    setAreaSize(areaInputs[0]);
    changed();
    showToast('Tablet area reset');
  });
  $('#conform-area-button').addEventListener('click', () => {
    display.width = Number(displayFields[0].value) || 1920;
    display.height = Number(displayFields[1].value) || 1080;
    const ratio = display.width / display.height;
    let width = tablet.width;
    let height = width / ratio;
    if (height > tablet.height) { height = tablet.height; width = height * ratio; }
    areaInputs[2].value = (tablet.width / 2).toFixed(1);
    areaInputs[3].value = (tablet.height / 2).toFixed(1);
    areaInputs[0].value = width.toFixed(1);
    areaInputs[1].value = height.toFixed(1);
    aspectSlider.value = ratio.toFixed(2);
    updateAreaShape();
    changed();
    showToast(`Area conformed to display ratio ${ratio.toFixed(2)}`);
  });
  updateAreaShape();
  new ResizeObserver(() => updateAreaShape()).observe(areaStage);
  document.querySelectorAll('.display-fields input, .binding-row select, .switch').forEach((control) => {
    control.addEventListener('input', () => {
      if (control === displayFields[0]) display.width = Number(control.value) || 1;
      if (control === displayFields[1]) display.height = Number(control.value) || 1;
      $('#display-ratio-action').textContent = `${(display.width / display.height).toFixed(2)}`;
      changed();
    });
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
    display.width = 1920;
    display.height = 1080;
    areaInputs.forEach((input, i) => input.value = ['76', '43', '76.2', '50.8', '0', '0'][i]);
    aspectSlider.value = '1.77';
    aspectLock.checked = false;
    updateAreaShape();
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
