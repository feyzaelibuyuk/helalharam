// Multipart browser bridge. All validation and storage remain on the C# server.
document.addEventListener('submit', async event => {
  const form=event.target;
  if(!(form instanceof HTMLFormElement)||!form.matches('[data-evidence-form]'))return;
  event.preventDefault();
  const status=form.querySelector('[data-evidence-status]');
  const button=form.querySelector('button[type="submit"]');
  if(button.disabled)return;
  button.disabled=true;status.textContent='Gönderiliyor…';
  try {
    const response=await fetch(form.action,{method:'POST',body:new FormData(form),credentials:'same-origin',headers:{Accept:'application/json'}});
    const data=await response.json();status.textContent=data.message??'Gönderim işlenemedi.';
    if(response.ok){status.textContent+=' Takip için gönderim kimliği: '+data.id;form.querySelectorAll('input,select,textarea').forEach(x=>x.disabled=true);}
    else button.disabled=false;
  }catch{status.textContent='Bağlantı kurulamadı. Girdileriniz korunuyor; aynı gönderimi tekrar deneyebilirsiniz.';button.disabled=false;}
});
