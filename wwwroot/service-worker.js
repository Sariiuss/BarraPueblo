const CACHE_NAME='barra-valdorros-v4';
const INDEX_URL=new URL('index.html',self.location.href).href;
const APP_SHELL=['./','index.html','manifest.webmanifest','app-icon.svg','escudo-valdorros.png'].map(path=>new URL(path,self.location.href).href);

self.addEventListener('install',event=>{
  event.waitUntil(caches.open(CACHE_NAME).then(cache=>cache.addAll(APP_SHELL)).then(()=>self.skipWaiting()));
});

self.addEventListener('activate',event=>{
  event.waitUntil(caches.keys().then(keys=>Promise.all(keys.filter(key=>key!==CACHE_NAME).map(key=>caches.delete(key)))).then(()=>self.clients.claim()));
});

self.addEventListener('fetch',event=>{
  if(event.request.method!=='GET'||new URL(event.request.url).origin!==self.location.origin)return;

  if(event.request.mode==='navigate'){
    event.respondWith(caches.match(INDEX_URL).then(cached=>cached||fetch(event.request)));
    return;
  }

  event.respondWith(caches.match(event.request).then(cached=>cached||fetch(event.request).then(response=>{
    const copy=response.clone();
    caches.open(CACHE_NAME).then(cache=>cache.put(event.request,copy));
    return response;
  })));
});
