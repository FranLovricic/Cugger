/* ========== CUGGER — Scroll Animations & Interactions ========== */

(function () {
  'use strict';

  // ── Scroll Reveal (IntersectionObserver) ──
  function initScrollReveal() {
    var els = document.querySelectorAll('.reveal, .reveal-left, .reveal-right, .reveal-scale, .stagger');
    if (!els.length) return;

    var observer = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible');
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

    els.forEach(function (el) { observer.observe(el); });
  }

  // ── Navbar scroll effect ──
  function initNavbar() {
    var navbar = document.querySelector('.navbar');
    if (!navbar) return;

    function check() {
      if (window.scrollY > 60) {
        navbar.classList.add('scrolled');
      } else {
        navbar.classList.remove('scrolled');
      }
    }
    check();
    window.addEventListener('scroll', check, { passive: true });
  }

  // ── Animated counters ──
  function initCounters() {
    var counters = document.querySelectorAll('[data-counter]');
    if (!counters.length) return;

    var observer = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          animateCounter(entry.target);
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.5 });

    counters.forEach(function (el) { observer.observe(el); });
  }

  function animateCounter(el) {
    var target = parseInt(el.getAttribute('data-counter'), 10);
    var suffix = el.getAttribute('data-suffix') || '';
    var duration = 1200;
    var start = performance.now();

    function step(now) {
      var elapsed = now - start;
      var progress = Math.min(elapsed / duration, 1);
      // easeOutExpo
      var ease = progress === 1 ? 1 : 1 - Math.pow(2, -10 * progress);
      el.textContent = Math.round(target * ease) + suffix;
      if (progress < 1) requestAnimationFrame(step);
    }
    requestAnimationFrame(step);
  }

  // ── Smooth hover tilt on cards ──
  function initCardTilt() {
    var cards = document.querySelectorAll('.card, .checkin-card');
    cards.forEach(function (card) {
      card.addEventListener('mousemove', function (e) {
        var rect = card.getBoundingClientRect();
        var x = (e.clientX - rect.left) / rect.width - 0.5;
        var y = (e.clientY - rect.top) / rect.height - 0.5;
        card.style.transform = 'translateY(-4px) perspective(800px) rotateX(' + (y * -3) + 'deg) rotateY(' + (x * 3) + 'deg)';
      });
      card.addEventListener('mouseleave', function () {
        card.style.transform = '';
      });
    });
  }

  // ── Marquee pause on hover ──
  function initMarquee() {
    var wraps = document.querySelectorAll('.marquee-wrap');
    wraps.forEach(function (wrap) {
      var marquee = wrap.querySelector('.marquee');
      if (!marquee) return;
      wrap.addEventListener('mouseenter', function () {
        marquee.style.animationPlayState = 'paused';
      });
      wrap.addEventListener('mouseleave', function () {
        marquee.style.animationPlayState = 'running';
      });
    });
  }

  // ── Mobile hamburger menu ──
  function initMobileNav() {
    var toggle = document.getElementById('navbar-toggle');
    var menu = document.getElementById('navbar-menu');
    if (!toggle || !menu) return;

    function setOpen(open) {
      menu.classList.toggle('open', open);
      toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
      toggle.setAttribute('aria-label', open ? 'Zatvori izbornik' : 'Otvori izbornik');
    }

    toggle.addEventListener('click', function () {
      setOpen(!menu.classList.contains('open'));
    });

    // Zatvori nakon odabira stavke ili povratka na desktop širinu
    menu.addEventListener('click', function (e) {
      if (e.target.closest('a, button')) setOpen(false);
    });
    window.addEventListener('resize', function () {
      if (window.innerWidth > 900) setOpen(false);
    });
  }

  // ── Init ──
  document.addEventListener('DOMContentLoaded', function () {
    initScrollReveal();
    initNavbar();
    initCounters();
    initCardTilt();
    initMarquee();
    initMobileNav();
  });
})();
