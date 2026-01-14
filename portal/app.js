const state = {
  catalog: null,
  activeAttackId: null,
  activeAppId: null,
};

const elements = {
  attackList: document.querySelector("[data-attack-list]"),
  attackTitle: document.querySelector("[data-attack-title]"),
  attackTags: document.querySelector("[data-attack-tags]"),
  attackSummary: document.querySelector("[data-attack-summary]"),
  attackVariants: document.querySelector("[data-attack-variants]"),
  appList: document.querySelector("[data-app-list]"),
  launchLink: document.querySelector("[data-attack-launch]"),
  launchNote: document.querySelector("[data-attack-launch-note]"),
};

const statusLabels = {
  live: "Live",
  roadmap: "Roadmap",
  planned: "Planned",
};

const safeText = (value) => (value ? String(value) : "");

const pickDefaultApp = (attack, apps) => {
  const supported = (attack.supportedApps || []).map((id) => id.toLowerCase());
  const match = apps.find((app) => supported.includes(app.id.toLowerCase()));
  return match || apps[0];
};

const renderAttackList = (attacks) => {
  elements.attackList.innerHTML = "";
  attacks.forEach((attack) => {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "attack-button";
    button.dataset.attackId = attack.id;
    button.innerHTML = `
      <span class="attack-name">${safeText(attack.name)}</span>
      <span class="attack-meta">${safeText(attack.summary)}</span>
    `;
    button.addEventListener("click", () => setActiveAttack(attack.id));
    elements.attackList.appendChild(button);
  });
};

const renderTags = (attack) => {
  const tags = attack.tags || [];
  elements.attackTags.innerHTML = "";
  tags.forEach((tag) => {
    const span = document.createElement("span");
    span.className = "pill";
    span.textContent = tag;
    elements.attackTags.appendChild(span);
  });
};

const renderVariants = (attack) => {
  elements.attackVariants.innerHTML = "";
  (attack.variants || []).forEach((variant) => {
    const card = document.createElement("div");
    card.className = "variant-card";
    card.innerHTML = `
      <div class="variant-title">${safeText(variant.name)}</div>
      <div class="variant-description">${safeText(variant.description)}</div>
      <code>${safeText(variant.example)}</code>
    `;
    elements.attackVariants.appendChild(card);
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
    elements.launchNote.textContent = "Choose an attack type to see which stacks are mapped.";
  }
};

const renderAttackDetails = (attack, apps) => {
  elements.attackTitle.textContent = safeText(attack.name || "Choose an attack");
  elements.attackSummary.textContent = safeText(attack.summary || "");
  renderTags(attack);
  renderVariants(attack);
  renderApps(attack, apps);
  renderLaunch(attack, apps);

  const buttons = elements.attackList.querySelectorAll(".attack-button");
  buttons.forEach((button) => {
    const isActive = button.dataset.attackId === state.activeAttackId;
    button.classList.toggle("active", isActive);
    button.setAttribute("aria-pressed", String(isActive));
  });

  const appButtons = elements.appList.querySelectorAll(".app-chip");
  appButtons.forEach((button) => {
    const isActive = button.dataset.appId === state.activeAppId;
    button.classList.toggle("active", isActive);
    button.setAttribute("aria-pressed", String(isActive));
  });
};

const setActiveAttack = (attackId) => {
  state.activeAttackId = attackId;
  const attack = state.catalog.attacks.find((item) => item.id === attackId) || {};
  const apps = state.catalog.applications || [];
  const defaultApp = pickDefaultApp(attack, apps);
  state.activeAppId = defaultApp ? defaultApp.id : null;
  renderAttackDetails(attack, apps);
};

const setActiveApp = (appId) => {
  state.activeAppId = appId;
  const attack = state.catalog.attacks.find((item) => item.id === state.activeAttackId) || {};
  renderAttackDetails(attack, state.catalog.applications || []);
};

const initCatalog = (catalog) => {
  state.catalog = catalog;
  renderAttackList(catalog.attacks || []);
  const firstAttack = (catalog.attacks || [])[0];
  if (firstAttack) {
    setActiveAttack(firstAttack.id);
  }
};

const showError = () => {
  elements.attackList.innerHTML = "<p class=\"muted-text\">Unable to load attack catalog.</p>";
};

fetch("data/attacks.json")
  .then((response) => {
    if (!response.ok) {
      throw new Error("Catalog fetch failed");
    }
    return response.json();
  })
  .then(initCatalog)
  .catch(showError);
