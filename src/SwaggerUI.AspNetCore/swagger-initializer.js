window.onload = function () {
  try {
    let options = {
      dom_id: '#swagger-ui',
      deepLinking: true,
      presets: [
        SwaggerUIBundle.presets.apis,
        SwaggerUIStandalonePreset
      ],
      plugins: [
        SwaggerUIBundle.plugins.DownloadUrl
      ],
      layout: "StandaloneLayout"
    };

    /** OptionsSetupSnippet */

    /** CustomCodeBeforeInitialization */

    let customOptions;

    /** CustomConfigurationSnippet */

    window.ui = SwaggerUIBundle({ ...options, ...customOptions });
  } catch (error) {
    alert(`swagger-ui init failed. Please check configurations. Error:\n "${error}"`);
    throw error;
  }
};
