import React from 'react';

type ErrorStateProps = {
  message: string;
  onRetry?: () => void;
};

const ErrorState: React.FC<ErrorStateProps> = ({ message, onRetry }) => (
  <div style={{ padding: '2rem', textAlign: 'center', color: '#e74c3c' }}>
    <p>Erreur : {message}</p>
    {onRetry && (
      <button onClick={onRetry} style={{ marginTop: '0.5rem', cursor: 'pointer' }}>
        Réessayer
      </button>
    )}
  </div>
);

export default ErrorState;