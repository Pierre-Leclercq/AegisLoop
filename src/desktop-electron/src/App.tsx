import React from 'react';
import { Routes, Route, NavLink } from 'react-router-dom';
import Dashboard from './views/Dashboard';
import MapTimeline from './views/MapTimeline';
import EventCase from './views/EventCase';
import Observations from './views/Observations';
import Settings from './views/Settings';

// Navigation conforme aux 5 vues V1 officielles
const navItems = [
  { path: '/', label: 'Dashboard', shortcut: 'Ctrl+1' },
  { path: '/map', label: 'Carte + Timeline', shortcut: 'Ctrl+2' },
  { path: '/event-case', label: 'EventCase', shortcut: 'Ctrl+3' },
  { path: '/observations', label: 'Observations', shortcut: 'Ctrl+4' },
  { path: '/settings', label: 'Paramètres', shortcut: 'Ctrl+5' },
];

const App: React.FC = () => {
  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      {/* Sidebar */}
      <nav style={{
        width: '220px',
        background: 'var(--bg-secondary)',
        padding: '1rem 0',
        borderRight: '1px solid #2a2a3e',
      }}>
        <div style={{
          padding: '0 1rem 1rem',
          fontSize: '1.2rem',
          fontWeight: 'bold',
          color: 'var(--accent)',
          borderBottom: '1px solid #2a2a3e',
          marginBottom: '0.5rem',
        }}>
          AegisLoop V1
        </div>
        {navItems.map(item => (
          <NavLink
            key={item.path}
            to={item.path}
            end={item.path === '/'}
            style={({ isActive }) => ({
              display: 'block',
              padding: '0.6rem 1rem',
              color: isActive ? 'var(--accent)' : 'var(--text-secondary)',
              textDecoration: 'none',
              background: isActive ? 'rgba(74, 144, 217, 0.1)' : 'transparent',
              borderLeft: isActive ? '3px solid var(--accent)' : '3px solid transparent',
            })}
          >
            {item.label}
          </NavLink>
        ))}
      </nav>

      {/* Main content */}
      <main style={{ flex: 1, padding: '1.5rem' }}>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/map" element={<MapTimeline />} />
          <Route path="/event-case" element={<EventCase />} />
          <Route path="/observations" element={<Observations />} />
          <Route path="/settings" element={<Settings />} />
        </Routes>
      </main>
    </div>
  );
};

export default App;