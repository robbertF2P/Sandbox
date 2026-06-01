const defaultTargetUrl = 'https://2025-14-patch.floor2plan.com/Account/Login';

const splitTargetUrls = () => {
  const configuredUrls = Cypress.env('targetUrls') || Cypress.env('targetUrl') || defaultTargetUrl;

  return configuredUrls
    .split(',')
    .map((url) => url.trim())
    .filter(Boolean);
};

const emailSelectors = [
  'input[type="email"]',
  'input[name*="email"]',
  'input[name*="Email"]',
  'input[name*="username"]',
  'input[name*="Username"]',
  'input[name*="userName"]',
  'input[name*="UserName"]',
  'input[id*="email"]',
  'input[id*="Email"]',
  'input[id*="username"]',
  'input[id*="Username"]',
  'input[id*="userName"]',
  'input[id*="UserName"]',
  'input[placeholder*="email"]',
  'input[placeholder*="Email"]',
  'input[placeholder*="username"]',
  'input[placeholder*="Username"]',
].join(',');

splitTargetUrls().forEach((targetUrl) => {
  describe(`Floor2Plan login smoke test: ${targetUrl}`, () => {
    it('loads the login page and accepts email entry after the lower-right click', () => {
      const smokeEmail = Cypress.env('smokeEmail') || 'smoke@example.com';

      cy.visit(targetUrl);

      cy.location('href', { timeout: 30000 }).should('include', '/Account/Login');
      cy.get('body').should('be.visible').and('not.be.empty');

      cy.log('Click lower-right body corner');
      cy.get('body').click('bottomRight', { force: true });

      cy.get(emailSelectors)
        .filter(':visible')
        .first()
        .should('exist')
        .clear()
        .type(smokeEmail)
        .should('have.value', smokeEmail);

      cy.screenshot('login-email-entered');

      const visualSettleMs = Number(Cypress.env('visualSettleMs') || 0);
      if (visualSettleMs > 0) {
        cy.wait(visualSettleMs);
      }
    });
  });
});
