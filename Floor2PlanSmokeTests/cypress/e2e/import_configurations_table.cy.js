const {
  openSystemTable,
  resolveAdditionalConfigColumn,
  cellText,
  beginInlineEdit,
  activeInlineEditor,
  cancelInlineEdit,
  assertNoInlineEditor,
  gridDataRows,
} = require('../helpers/system-tables');

const IMPORT_CONFIG_TABLE = 'ImportConfig';
const EDIT_PROBE_VALUE = `smoke-probe-${Date.now()}`;

describe('Import Configuration system table', () => {
  beforeEach(() => {
    openSystemTable(IMPORT_CONFIG_TABLE, 'import configuration');
  });

  it('opens Import Configuration from Application Settings system tables', () => {
    cy.location('pathname', { timeout: 30000 }).should('include', `/tables/${IMPORT_CONFIG_TABLE}`);
    cy.contains('Tables - Import Configuration').should('be.visible');
    gridDataRows().should('have.length.at.least', 1);
  });

  it('exposes the Additional config column on the Import Configuration grid', () => {
    resolveAdditionalConfigColumn();
  });

  it('cancels Additional config inline edit without persisting changes', () => {
    resolveAdditionalConfigColumn().then(({ columnIndex }) => {
      cellText(0, columnIndex).then((originalValue) => {
        beginInlineEdit(0, columnIndex);
        activeInlineEditor()
          .clear({ force: true })
          .type(EDIT_PROBE_VALUE, { force: true });

        cancelInlineEdit();
        assertNoInlineEditor();

        cellText(0, columnIndex).should((value) => {
          expect(value, 'cancelled edit value').to.equal(originalValue);
          expect(value, 'probe must not be saved').to.not.include(EDIT_PROBE_VALUE);
        });
      });
    });
  });

  it('discards Additional config edits when selecting another row', () => {
    resolveAdditionalConfigColumn().then(({ columnIndex }) => {
      cellText(0, columnIndex).then((originalValue) => {
        beginInlineEdit(0, columnIndex);
        activeInlineEditor()
          .clear({ force: true })
          .type(EDIT_PROBE_VALUE, { force: true });

        gridDataRows().eq(1).click({ force: true });
        cy.wait(300);

        assertNoInlineEditor();

        cellText(0, columnIndex).should((value) => {
          expect(value, 'row 1 value after switching rows').to.equal(originalValue);
          expect(value, 'probe must not be saved').to.not.include(EDIT_PROBE_VALUE);
        });
      });
    });
  });

  it('leaves edit mode after cancel and moves focus to another row', () => {
    resolveAdditionalConfigColumn().then(({ columnIndex }) => {
      beginInlineEdit(0, columnIndex);
      activeInlineEditor().type('{selectall}temporary-edit', { force: true });

      cancelInlineEdit();
      gridDataRows().eq(1).click({ force: true });

      assertNoInlineEditor();
      gridDataRows().eq(1).should('have.class', 'k-selected');
    });
  });
});
