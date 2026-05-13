import React from 'react';

type ScoreBadgeProps = {
  score: number;
};

const scoreColor = (value: number): string => {
  if (value >= 0.7) return '#27ae60';
  if (value >= 0.4) return '#f39c12';
  return '#e74c3c';
};

const ScoreBadge: React.FC<ScoreBadgeProps> = ({ score }) => (
  <span
    style={{
      display: 'inline-block',
      padding: '0.15rem 0.5rem',
      borderRadius: '4px',
      background: scoreColor(score),
      color: '#fff',
      fontWeight: 600,
      fontSize: '0.8rem',
    }}
  >
    {(score * 100).toFixed(0)}%
  </span>
);

export default ScoreBadge;