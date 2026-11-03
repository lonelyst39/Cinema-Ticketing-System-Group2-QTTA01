(function () {
  'use strict';
  var $ = function (s, c) { return (c || document).querySelector(s); };
  var $$ = function (s, c) { return Array.prototype.slice.call((c || document).querySelectorAll(s)); };
  var reduce = window.matchMedia && matchMedia('(prefers-reduced-motion: reduce)').matches;

  // Màn nhung: chỉ chạy một lần mỗi phiên
  var drape = $('.drape');
  if (drape) {
    var done = function () {
      document.documentElement.classList.remove('has-curtain');
      try { sessionStorage.setItem('curtain', '1'); } catch (e) {}
      drape.remove();
    };
    drape.lastElementChild.addEventListener('animationend', done);
    setTimeout(done, 2400);
  }

  // Menu mobile
  var tg = $('.nav-toggle'), nav = $('.nav');
  if (tg && nav) tg.addEventListener('click', function () {
    var open = nav.classList.toggle('open'); tg.setAttribute('aria-expanded', open);
  });

  // Toast tự tắt
  $$('.toast').forEach(function (t) {
    var close = function () { t.classList.add('leave'); setTimeout(function () { t.remove(); }, 320); };
    t.addEventListener('click', close); setTimeout(close, 6000);
  });

  // Hero carousel
  var hero = $('.hero');
  if (hero) {
    var slides = $$('.slide', hero), dots = $$('.dot', hero), cur = 0, timer, DUR = 6500;
    var go = function (n) {
      cur = (n + slides.length) % slides.length;
      slides.forEach(function (s, i) { s.classList.toggle('on', i === cur); s.setAttribute('aria-hidden', i !== cur); });
      dots.forEach(function (d, i) {
        d.classList.remove('on'); void d.offsetWidth; d.classList.toggle('on', i === cur);
        d.style.setProperty('--dur', DUR + 'ms');
      });
    };
    var play = function () { clearInterval(timer); if (!reduce && slides.length > 1) timer = setInterval(function () { go(cur + 1); }, DUR); };
    dots.forEach(function (d, i) { d.addEventListener('click', function () { go(i); play(); }); });
    hero.addEventListener('mouseenter', function () { clearInterval(timer); });
    hero.addEventListener('mouseleave', play);
    go(0); play();
  }

  // Sao chép mã khuyến mãi
  $$('[data-copy]').forEach(function (b) { b.addEventListener('click', function () {
    var v = b.dataset.copy, old = b.textContent;
    var ok = function () { b.textContent = 'Đã chép'; setTimeout(function () { b.textContent = old; }, 1600); };
    if (navigator.clipboard) navigator.clipboard.writeText(v).then(ok, ok); else ok();
  }); });

  // Hiện/ẩn mật khẩu
  $$('[data-toggle-pw]').forEach(function (b) { b.addEventListener('click', function () {
    var i = b.parentElement.querySelector('input'); i.type = i.type === 'password' ? 'text' : 'password';
    b.firstElementChild.className = 'fa ' + (i.type === 'password' ? 'fa-eye' : 'fa-eye-slash');
  }); });
})();
