const defaultTargetUrl = 'https://2025-14-patch.floor2plan.com/Account/Login';

const splitTargetUrls = () => {
  const configuredUrls = Cypress.env('targetUrls') || Cypress.env('targetUrl') || defaultTargetUrl;

  return configuredUrls
    .split(',')
    .map((url) => url.trim())
    .filter(Boolean);
};

const tileSelectors = [
  '.f2ps-tile',
  '[class*="tile"]',
  '[class*="Tile"]',
  '[data-testid*="tile"]',
  '[data-testid*="Tile"]',
].join(',');

const homeTileSelector = () => Cypress.env('homeTileSelector') || tileSelectors;

splitTargetUrls().forEach((targetUrl) => {
  describe(`Floor2Plan login smoke test: ${targetUrl}`, () => {
    it('enters through the lower-right Floorganise logo and renders home tiles', () => {
      const minHomeTiles = Number(Cypress.env('minHomeTiles') || 2);

      cy.on('fail', (error) => {
        if (error.message.includes('remote page to load')) {
          throw new Error(
            'The Floorganise logo started Azure AD navigation, but this runner does not have an authenticated Microsoft SSO session for the target application. Home tiles can only be verified when that session is available.',
          );
        }

        throw error;
      });

      cy.visit(targetUrl);

      cy.location('href', { timeout: 30000 }).should('include', '/Account/Login');
      cy.get('body').should('be.visible').and('not.be.empty');

      cy.log('Click lower-right Floorganise logo');
      cy.get('#azure-login img[alt="Floorganise logo"]')
        .should('be.visible')
        .click({ force: true });

      cy.location('pathname', { timeout: 30000 }).should('not.include', '/Account/Login');
      cy.location('hostname').should('eq', new URL(targetUrl).hostname);
      cy.get(homeTileSelector(), { timeout: 30000 })
        .filter(':visible')
        .should('have.length.at.least', minHomeTiles);

      cy.screenshot('home-tiles-rendered');

      const visualSettleMs = Number(Cypress.env('visualSettleMs') || 0);
      if (visualSettleMs > 0) {
        cy.wait(visualSettleMs);
      }
    });
  });
});
