import { useNavigate } from "react-router-dom";
import logoPng from '../assets/SPM-Logo.png';
import './styles/PreferencesIntroPage.css';

const PreferencesIntroPage = () => {
  const navigate = useNavigate();

  const handleStartQuestionnaire = () => {
    navigate("/login/preferences");
  };

  const handleDoItLater = () => {
    navigate("/login/dashboard");
  };

  return (
    <div className="preferences-intro-container">
      {/* Icon / Illustration */}
      <div className="preferences-intro-icon">
        <img
          src={logoPng}
          alt="Preferences onboarding"
        />
      </div>

      {/* Main message */}
      <h2 className="preferences-intro-title">
        Please take some time to optimize your search results!
      </h2>

      {/* Primary action */}
      <button
        className="preferences-intro-start"
        onClick={handleStartQuestionnaire}
      >
        Start Questionnaire
      </button>

      {/* Secondary message */}
      <p className="preferences-intro-description">
        No time yet? Jump over and start it later from the settings section.
      </p>

      {/* Secondary action */}
      <button
        className="preferences-intro-later"
        onClick={handleDoItLater}
      >
        Do it later
      </button>
    </div>
  );
};

export default PreferencesIntroPage;
