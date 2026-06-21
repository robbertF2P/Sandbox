// apps/f2p-shell/src/app/app.routes.ts — lazy route per bounded context

{
  path: '<context>',
  loadChildren: () =>
    import('@f2p/<context>/feature-status').then(m => m.<context>Routes),
},
