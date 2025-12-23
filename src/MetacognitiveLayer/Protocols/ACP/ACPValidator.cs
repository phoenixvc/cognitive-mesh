using System.Xml;
using System.Xml.Schema;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.ACP
{
    /// <summary>
    /// Validates ACP (AI Communication Protocol) XML templates against the schema.
    /// </summary>
    public class ACPValidator
    {
        private readonly ILogger<ACPValidator> _logger;
        private XmlSchemaSet _schemaSet;
        private bool _hasValidationErrors;

        public ACPValidator(ILogger<ACPValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeSchema();
        }

        /// <summary>
        /// Initializes the ACP XML schema for validation.
        /// </summary>
        private void InitializeSchema()
        {
            try
            {
                _logger.LogInformation("Initializing ACP XML schema");
                
                _schemaSet = new XmlSchemaSet();
                
                // Define the ACP schema inline
                string schemaXml = @"<?xml version='1.0' encoding='UTF-8'?>
                <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified'>
                  <xs:element name='Task'>
                    <xs:complexType>
                      <xs:sequence>
                        <xs:element name='Description' type='xs:string' minOccurs='0'/>
                        <xs:element name='PrimaryTool' type='xs:string'/>
                        <xs:element name='Parameters' minOccurs='0'>
                          <xs:complexType>
                            <xs:sequence>
                              <xs:element name='Parameter' maxOccurs='unbounded'>
                                <xs:complexType>
                                  <xs:sequence>
                                    <xs:element name='Value' type='xs:anyType' minOccurs='0'/>
                                    <xs:element name='Description' type='xs:string' minOccurs='0'/>
                                  </xs:sequence>
                                  <xs:attribute name='name' type='xs:string' use='required'/>
                                  <xs:attribute name='type' type='xs:string' use='required'/>
                                  <xs:attribute name='required' type='xs:boolean' default='false'/>
                                  <xs:attribute name='isContextual' type='xs:boolean' default='false'/>
                                </xs:complexType>
                              </xs:element>
                            </xs:sequence>
                          </xs:complexType>
                        </xs:element>
                        <xs:element name='Constraints' minOccurs='0'>
                          <xs:complexType>
                            <xs:sequence>
                              <xs:element name='Constraint' maxOccurs='unbounded'>
                                <xs:complexType>
                                  <xs:sequence>
                                    <xs:element name='Value' type='xs:string'/>
                                    <xs:element name='ErrorMessage' type='xs:string' minOccurs='0'/>
                                  </xs:sequence>
                                  <xs:attribute name='type' type='xs:string' use='required'/>
                                  <xs:attribute name='parameterName' type='xs:string' use='required'/>
                                  <xs:attribute name='severity' type='xs:string' default='error'/>
                                </xs:complexType>
                              </xs:element>
                            </xs:sequence>
                          </xs:complexType>
                        </xs:element>
                        <xs:element name='RequiredTools' minOccurs='0'>
                          <xs:complexType>
                            <xs:sequence>
                              <xs:element name='Tool' type='xs:string' maxOccurs='unbounded'/>
                            </xs:sequence>
                          </xs:complexType>
                        </xs:element>
                        <xs:element name='Fallback' minOccurs='0'>
                          <xs:complexType>
                            <xs:sequence>
                              <xs:element name='AlternateTask' type='xs:string' minOccurs='0'/>
                              <xs:element name='DefaultResponse' type='xs:string' minOccurs='0'/>
                            </xs:sequence>
                            <xs:attribute name='type' type='xs:string' use='required'/>
                            <xs:attribute name='maxRetries' type='xs:int' default='0'/>
                            <xs:attribute name='logDetailedFailure' type='xs:boolean' default='true'/>
                          </xs:complexType>
                        </xs:element>
                        <xs:element name='Metadata' minOccurs='0'>
                          <xs:complexType>
                            <xs:sequence>
                              <xs:element name='Author' type='xs:string' minOccurs='0'/>
                              <xs:element name='CreatedDate' type='xs:dateTime' minOccurs='0'/>
                              <xs:element name='LastModifiedDate' type='xs:dateTime' minOccurs='0'/>
                              <xs:element name='ParentTemplate' type='xs:string' minOccurs='0'/>
                              <xs:element name='Tags' minOccurs='0'>
                                <xs:complexType>
                                  <xs:sequence>
                                    <xs:element name='Tag' type='xs:string' maxOccurs='unbounded'/>
                                  </xs:sequence>
                                </xs:complexType>
                              </xs:element>
                              <xs:element name='CustomProperties' minOccurs='0'>
                                <xs:complexType>
                                  <xs:sequence>
                                    <xs:element name='Property' maxOccurs='unbounded'>
                                      <xs:complexType>
                                        <xs:attribute name='key' type='xs:string' use='required'/>
                                        <xs:attribute name='value' type='xs:string' use='required'/>
                                      </xs:complexType>
                                    </xs:element>
                                  </xs:sequence>
                                </xs:complexType>
                              </xs:element>
                            </xs:sequence>
                          </xs:complexType>
                        </xs:element>
                      </xs:sequence>
                      <xs:attribute name='name' type='xs:string' use='required'/>
                      <xs:attribute name='domain' type='xs:string' use='required'/>
                      <xs:attribute name='version' type='xs:string' use='required'/>
                    </xs:complexType>
                  </xs:element>
                </xs:schema>";
                
                using (var schemaReader = new StringReader(schemaXml))
                {
                    using (var xmlReader = XmlReader.Create(schemaReader))
                    {
                        _schemaSet.Add(null, xmlReader);
                    }
                }
                
                _schemaSet.Compile();
                _logger.LogInformation("ACP XML schema initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing ACP XML schema");
                throw;
            }
        }

        /// <summary>
        /// Validates an ACP XML template against the schema.
        /// </summary>
        public Task<bool> ValidateAsync(string templateXml)
        {
            try
            {
                _logger.LogDebug("Validating ACP XML template against schema");
                
                if (string.IsNullOrEmpty(templateXml))
                {
                    _logger.LogError("XML template is null or empty");
                    return Task.FromResult(false);
                }
                
                _hasValidationErrors = false;
                
                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    Schemas = _schemaSet,
                    ValidationEventHandler = ValidationEventHandler
                };
                
                using (var stringReader = new StringReader(templateXml))
                {
                    using (var xmlReader = XmlReader.Create(stringReader, settings))
                    {
                        while (xmlReader.Read())
                        {
                            // Reading the entire document for validation
                        }
                    }
                }
                
                return Task.FromResult(!_hasValidationErrors);
            }
            catch (XmlException ex)
            {
                _logger.LogError(ex, "XML parsing error during validation");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ACP XML template");
                throw;
            }
        }

        /// <summary>
        /// Handles XML validation errors.
        /// </summary>
        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            _hasValidationErrors = true;
            
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    _logger.LogError("ACP validation error: {Message}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    _logger.LogWarning("ACP validation warning: {Message}", e.Message);
                    break;
            }
        }
    }
}