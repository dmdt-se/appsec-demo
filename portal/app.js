const state = {
  catalog: null,
  activeAttackId: null,
  activeAppId: null,
  variantByAttackId: {},
  copyTimer: null,
};

const elements = {
  attackGrid: document.querySelector("[data-attack-grid]"),
  appList: document.querySelector("[data-app-list]"),
  launchLink: document.querySelector("[data-attack-launch]"),
  launchNote: document.querySelector("[data-attack-launch-note]"),
  activeAttackTitle: document.querySelector("[data-active-attack-title]"),
  activeVariantTitle: document.querySelector("[data-active-variant-title]"),
  activeVariantPayload: document.querySelector("[data-active-variant-payload]"),
  copyPayload: document.querySelector("[data-copy-payload]"),
};

const statusLabels = {
  live: "Live",
  roadmap: "Roadmap",
  planned: "Planned",
};

const safeText = (value) => (value ? String(value) : "");

const getAttack = (attackId) =>
  (state.catalog?.attacks || []).find((attack) => attack.id === attackId);

const getSelectedVariant = (attack) => {
  if (!attack) {
    return null;
  }
  const index = state.variantByAttackId[attack.id] ?? 0;
  return (attack.variants || [])[index] || null;
};

const pickDefaultApp = (attack, apps) => {
  const supported = (attack.supportedApps || []).map((id) => id.toLowerCase());
  const match = apps.find((app) => supported.includes(app.id.toLowerCase()));
  return match || apps[0];
};

const renderAttackCards = (attacks) => {
  elements.attackGrid.innerHTML = "";
  attacks.forEach((attack) => {
    const selectedIndex = state.variantByAttackId[attack.id] ?? 0;
    const selectedVariant = (attack.variants || [])[selectedIndex] || {};

    const card = document.createElement("article");
    card.className = "attack-card";
    card.dataset.attackId = attack.id;
    if (attack.id === state.activeAttackId) {
      card.classList.add("active");
    }

    card.innerHTML = `
      <div class="attack-card-header">
        <div>
          <h3 data-attack-name></h3>
          <p class="attack-card-summary" data-attack-summary></p>
        </div>
        <div class="attack-card-tags" data-attack-tags></div>
      </div>
      <div class="variant-label">Variant</div>
      <select class="variant-select" aria-label="Select attack variant"></select>
      <div class="variant-details">
        <p data-variant-description></p>
        <code data-variant-example></code>
      </div>
    `;

    card.querySelector("[data-attack-name]").textContent = safeText(attack.name);
    card.querySelector("[data-attack-summary]").textContent = safeText(attack.summary);

    const tagsWrap = card.querySelector("[data-attack-tags]");
    (attack.tags || []).forEach((tag) => {
      const tagEl = document.createElement("span");
      tagEl.className = "pill";
      tagEl.textContent = tag;
      tagsWrap.appendChild(tagEl);
    });

    const select = card.querySelector(".variant-select");
    (attack.variants || []).forEach((variant, index) => {
      const option = document.createElement("option");
      option.value = String(index);
      option.textContent = safeText(variant.name);
      if (index === selectedIndex) {
        option.selected = true;
      }
      select.appendChild(option);
    });

    card.querySelector("[data-variant-description]").textContent = safeText(selectedVariant.description);
    card.querySelector("[data-variant-example]").textContent = safeText(selectedVariant.example);

    card.addEventListener("click", () => setActiveAttack(attack.id));
    select.addEventListener("change", (event) => {
      state.variantByAttackId[attack.id] = Number(event.target.value || 0);
      setActiveAttack(attack.id);
    });

    elements.attackGrid.appendChild(card);
  });
};

const renderApps = (attack, apps) => {
  elements.appList.innerHTML = "";
  const supported = (attack.supportedApps || []).map((id) => id.toLowerCase());
  apps.forEach((app) => {
    const isSupported = supported.includes(app.id.toLowerCase());
    const status = app.status || "planned";
    const button = document.createElement("button");
    button.type = "button";
    button.className = "app-chip";
    button.dataset.appId = app.id;
    button.dataset.status = status;
    button.disabled = !isSupported;
    if (state.activeAppId === app.id) {
      button.classList.add("active");
    }
    button.innerHTML = `
      <span class="app-name">${safeText(app.name)}</span>
      <span class="app-status">${statusLabels[status] || "Planned"}</span>
    `;
    if (isSupported) {
      button.addEventListener("click", () => setActiveApp(app.id));
    }
    elements.appList.appendChild(button);
  });
};

const renderLaunch = (attack, apps) => {
  const app = apps.find((candidate) => candidate.id === state.activeAppId);
  const supported = (attack.supportedApps || []).map((id) => id.toLowerCase());
  const isSupported = app && supported.includes(app.id.toLowerCase());
  const isLive = app && app.status === "live" && app.path;

  if (app && isSupported && isLive) {
    elements.launchLink.textContent = `Launch ${app.name} demo`;
    elements.launchLink.href = app.path;
    elements.launchLink.classList.remove("disabled");
    elements.launchNote.textContent = safeText(app.notes || "Instrumented demo is available in the stack.");
  } else if (app && isSupported && !isLive) {
    elements.launchLink.textContent = `${app.name} demo planned`;
    elements.launchLink.href = "#";
    elements.launchLink.classList.add("disabled");
    elements.launchNote.textContent = "Scaffolded for future stacks. Swap in your own app and re-run scans.";
  } else {
    elements.launchLink.textContent = "Select a stack";
    elements.launchLink.href = "#";
    elements.launchLink.classList.add("disabled");
    elements.launchNote.textContent = "Choose an attack card to see which stacks are mapped.";
  }
};

const renderTargetPanel = (attack, apps) => {
  const variant = getSelectedVariant(attack);
  elements.activeAttackTitle.textContent = safeText(attack?.name || "Choose an attack");
  elements.activeVariantTitle.textContent = safeText(variant?.name || "Select a variant");
  elements.activeVariantPayload.textContent = safeText(variant?.example || "");
  renderApps(attack || {}, apps);
  renderLaunch(attack || {}, apps);
};

const setActiveAttack = (attackId) => {
  state.activeAttackId = attackId;
  const attack = getAttack(attackId) || {};
  const apps = state.catalog.applications || [];
  if (state.variantByAttackId[attackId] == null) {
    state.variantByAttackId[attackId] = 0;
  }

  const supported = (attack.supportedApps || []).map((id) => id.toLowerCase());
  if (!state.activeAppId || !supported.includes(state.activeAppId.toLowerCase())) {
    const defaultApp = pickDefaultApp(attack, apps);
    state.activeAppId = defaultApp ? defaultApp.id : null;
  }

  renderAttackCards(state.catalog.attacks || []);
  renderTargetPanel(attack, apps);
};

const setActiveApp = (appId) => {
  state.activeAppId = appId;
  const attack = getAttack(state.activeAttackId) || {};
  renderTargetPanel(attack, state.catalog.applications || []);
};

const setCopyFeedback = (message) => {
  if (!elements.copyPayload) {
    return;
  }
  elements.copyPayload.textContent = message;
  if (state.copyTimer) {
    clearTimeout(state.copyTimer);
  }
  state.copyTimer = setTimeout(() => {
    elements.copyPayload.textContent = "Copy payload";
  }, 1200);
};

const copyActivePayload = () => {
  const attack = getAttack(state.activeAttackId);
  const variant = getSelectedVariant(attack);
  const payload = variant?.example;
  if (!payload) {
    return;
  }

  if (navigator.clipboard && window.isSecureContext) {
    navigator.clipboard
      .writeText(payload)
      .then(() => setCopyFeedback("Copied"))
      .catch(() => setCopyFeedback("Copy failed"));
    return;
  }

  const textarea = document.createElement("textarea");
  textarea.value = payload;
  textarea.style.position = "fixed";
  textarea.style.opacity = "0";
  document.body.appendChild(textarea);
  textarea.select();
  try {
    document.execCommand("copy");
    setCopyFeedback("Copied");
  } catch (error) {
    setCopyFeedback("Copy failed");
  }
  document.body.removeChild(textarea);
};

const initCatalog = (catalog) => {
  state.catalog = catalog;
  const firstAttack = (catalog.attacks || [])[0];
  if (firstAttack) {
    setActiveAttack(firstAttack.id);
  }
};

const showError = () => {
  elements.attackGrid.innerHTML = "<p class=\"muted-text\">Unable to load attack catalog.</p>";
};

if (elements.copyPayload) {
  elements.copyPayload.addEventListener("click", copyActivePayload);
}

fetch("/data/attacks.json")
  .then((response) => {
    if (!response.ok) {
      throw new Error("Catalog fetch failed");
    }
    return response.json();
  })
  .then(initCatalog)
  .catch(showError);
