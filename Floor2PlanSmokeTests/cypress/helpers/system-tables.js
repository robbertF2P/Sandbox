const defaultOrigin = () => {
  const targetUrl = Cypress.env('targetUrls') || '';
  const first = targetUrl.split(',')[0]?.trim();
  if (first) {
    return new URL(first).origin;
  }

  return 'https://release-candidate.floor2plan.com';
};

const loginThroughServiceForm = () => {
  const username = Cypress.env('serviceUsername');
  const password = Cypress.env('servicePassword');

  expect(username, 'SMOKE_SERVICE_USERNAME').to.be.a('string').and.not.equal('');
  expect(password, 'SMOKE_SERVICE_PASSWORD').to.be.a('string').and.not.equal('');

  cy.get(Cypress.env('serviceUsernameSelector'), { timeout: 30000 })
    .should('be.visible')
    .clear()
    .type(username);
  cy.get(Cypress.env('servicePasswordSelector'), { timeout: 30000 })
    .should('be.visible')
    .clear()
    .type(password, { log: false });
  cy.get(Cypress.env('serviceSubmitSelector'), { timeout: 30000 })
    .filter(':visible')
    .first()
    .click({ force: true });

  cy.location('pathname', { timeout: 30000 }).should('not.include', '/Account/Login');
};

const ensureLoggedIn = (origin) => {
  cy.session(
    ['system-tables-login', origin, Cypress.env('serviceUsername')],
    () => {
      cy.visit(`${origin}/Account/Login`);
      loginThroughServiceForm();
    },
    {
      validate() {
        cy.visit(`${origin}/`);
        cy.location('pathname', { timeout: 15000 }).should('not.include', '/Account/Login');
      },
    },
  );
};

const openSystemTable = (tableIdentifier, searchTerm) => {
  const origin = defaultOrigin();
  ensureLoggedIn(origin);

  cy.intercept('GET', `**/System/Table/GetTableInfo/${tableIdentifier}`).as('tableInfo');
  cy.visit(`${origin}/system-administration/tables/${tableIdentifier}`);
  cy.get('.k-grid', { timeout: 30000 }).should('be.visible');
  cy.wait('@tableInfo', { timeout: 30000 });

  if (searchTerm) {
    cy.get('input')
      .filter((_, el) => /search/i.test(el.getAttribute('placeholder') || ''))
      .first()
      .clear({ force: true })
      .type(searchTerm, { force: true });
    cy.wait(1000);
  }
};

const additionalConfigColumnField = () => Cypress.env('additionalConfigColumnField') || 'additionalConfig';

const additionalConfigColumnTitle = () => new RegExp(
  Cypress.env('additionalConfigColumnTitle') || 'additional\\s*config',
  'i',
);

const resolveAdditionalConfigColumn = () => cy.get('@tableInfo').then(({ response }) => {
  const columns = response?.body?.columns || [];
  const field = additionalConfigColumnField();
  const titlePattern = additionalConfigColumnTitle();

  const match = columns.find((column) => column.field === field || titlePattern.test(column.title || ''));
  const available = columns.map((column) => `${column.field} (${column.title})`);

  expect(
    match,
    `Additional config column (${field}) in ImportConfig table. Available columns: ${available.join(', ')}`,
  ).to.exist;

  return cy.contains('th, [role="columnheader"]', titlePattern).scrollIntoView().should('be.visible').then(($header) => {
    const columnIndex = $header.index();
    return { field: match.field, title: match.title, columnIndex };
  });
});

const gridDataRows = () => cy.get('.k-grid-content tbody tr').filter(':visible');

const cellText = (rowIndex, columnIndex) => gridDataRows()
  .eq(rowIndex)
  .find('td')
  .eq(columnIndex)
  .invoke('text')
  .then((text) => text.trim());

const beginInlineEdit = (rowIndex, columnIndex) => {
  gridDataRows()
    .eq(rowIndex)
    .find('td')
    .eq(columnIndex)
    .scrollIntoView()
    .dblclick({ force: true });
};

const activeInlineEditor = () => cy
  .get('.k-grid-edit-row input:visible, .k-grid-edit-row textarea:visible, .k-edit-cell input:visible, .k-edit-cell textarea:visible, .k-edit-cell .k-input-inner:visible')
  .first();

const cancelInlineEdit = () => {
  cy.get('body').type('{esc}');
  cy.wait(300);
};

const assertNoInlineEditor = () => {
  cy.get('.k-grid-edit-row, .k-edit-cell').should('not.exist');
  cy.get('.k-grid-content input:visible, .k-grid-content textarea:visible').should('have.length', 0);
};

module.exports = {
  defaultOrigin,
  ensureLoggedIn,
  openSystemTable,
  resolveAdditionalConfigColumn,
  gridDataRows,
  cellText,
  beginInlineEdit,
  activeInlineEditor,
  cancelInlineEdit,
  assertNoInlineEditor,
};
