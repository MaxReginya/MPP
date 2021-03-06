﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace XMLSerializer
{
    class XMLSerializer
    {
        #region Private fields

        private readonly Assembly _assembly;
        private readonly XmlDocument _xmlDocument;

        #endregion

        #region Constructors

        public XMLSerializer(string assemblyName)
        {
            _xmlDocument = new XmlDocument();
            _assembly = Assembly.LoadFrom(assemblyName);
        }

        #endregion

        #region Public methods

        public XmlDocument Serialize()
        {
            var creator = new AssemblyClassesInfoCreator(_assembly);
            var classes = (List<ClassMembers>)creator.Create();
            CreateXmlTree(classes);
            return _xmlDocument;
        }

        #endregion

        #region Private methods
                
        private void CreateXmlTree(IEnumerable<ClassMembers> classes)
        {
            var classesList = (List<ClassMembers>)classes;            

            XmlNode assemblyNode = _xmlDocument.CreateElement("assembly");
            CreateAttribute("name", _assembly.FullName, assemblyNode);
            _xmlDocument.AppendChild(assemblyNode);

            foreach (var classMembers in classesList)
            {
                var node = CreateXmlNode(classMembers);
                assemblyNode.AppendChild(node);
            }                        
        }        

        private XmlNode CreateXmlNode(ClassMembers classMembers)
        {
            XmlNode classNode = CreateNamespace(classMembers);
            classNode.AppendChild(CreateClassName(classMembers));
            classNode.AppendChild(CreateInheritors(classMembers));
            classNode.AppendChild(CreateClassFields(classMembers));
            classNode.AppendChild(CreateMethods(classMembers));
            return classNode;
        }

        private void CreateAttribute(string attrName, string attrValue, XmlNode node)
        {
            var attr = _xmlDocument.CreateAttribute(attrName);
            attr.Value = attrValue;
            node.Attributes.Append(attr);
        }

        private XmlNode CreateNamespace(ClassMembers classMembers)
        {
            XmlNode namespaceNode = _xmlDocument.CreateElement("namespace");
            CreateAttribute("name", classMembers.Namespace, namespaceNode);
            return namespaceNode;
        }

        private XmlNode CreateClassName(ClassMembers classMembers)
        {
            XmlNode classNode = _xmlDocument.CreateElement("class");
            classNode.InnerText = classMembers.ClassName;
            return classNode;
        }

        private XmlNode CreateInheritors(ClassMembers classMembers)
        {
            XmlNode inheritorsNode = _xmlDocument.CreateElement("inheritors");
            foreach (var inheritorName in classMembers.Inheritors)
            {
                XmlNode inheritorNode = _xmlDocument.CreateElement("inheritor");
                inheritorNode.InnerText = inheritorName;
                inheritorsNode.AppendChild(inheritorNode);
            }
            return inheritorsNode;
        }

        private XmlNode CreateClassFields(ClassMembers classMembers)
        {
            XmlNode fieldsNode = _xmlDocument.CreateElement("fields");
            var usualFieldsNodes = CreateUsualFieldsNodes(classMembers);
            foreach (var usualNode in usualFieldsNodes)
            {
                fieldsNode.AppendChild(usualNode);
            }
            var childClassFieldsNodes = CreateChildClassFieldsNodes(classMembers);
            foreach (var childClassNode in childClassFieldsNodes)
            {
                fieldsNode.AppendChild(childClassNode);
            }
            return fieldsNode;
        }

        private IEnumerable<XmlNode> CreateUsualFieldsNodes(ClassMembers classMembers)
        {
            var list = new List<XmlNode>();
            foreach (var usualField in classMembers.UsualFields)
            {
                XmlNode usualFieldNode = _xmlDocument.CreateElement("field");
                CreateAttribute("name", usualField.Name, usualFieldNode);
                CreateAttribute("type", usualField.Type.Name, usualFieldNode);
                CreateAttribute("access_modifier", usualField.AccessModifier, usualFieldNode);
                list.Add(usualFieldNode);
            }
            return list;
        }

        private IEnumerable<XmlNode> CreateChildClassFieldsNodes(ClassMembers classMembers)
        {
            var list = new List<XmlNode>();
            foreach (var childClassField in classMembers.ChildClassFields)
            {
                XmlNode childClassFieldNode = _xmlDocument.CreateElement("field");
                CreateAttribute("name", childClassField.Name, childClassFieldNode);
                CreateAttribute("access_modifier", childClassField.AccessModifier, childClassFieldNode);
                if (childClassField.Type != null)
                {
                    var childClassNode = CreateXmlNode(childClassField.Type);
                    childClassFieldNode.AppendChild(childClassNode);
                }
                list.Add(childClassFieldNode);
            }
            return list;
        }

        private XmlNode CreateMethods(ClassMembers classMembers)
        {
            XmlNode methodsNode = _xmlDocument.CreateElement("methods");
            foreach (var method in classMembers.Methods)
            {
                XmlNode methodNode = _xmlDocument.CreateElement("method");
                CreateAttribute("name", method.Name, methodNode);
                CreateAttribute("access_modifier", method.AccessModifier, methodNode);
                CreateAttribute("return_type", method.ReturnType.Name, methodNode);                
                XmlNode paramsNode = _xmlDocument.CreateElement("params");                
                foreach (var param in method.Params)
                {
                    XmlNode paramNode = _xmlDocument.CreateElement("param");                    
                    CreateAttribute("name", param.Name, paramNode);
                    CreateAttribute("type", param.Type.Name, paramNode);
                    paramsNode.AppendChild(paramNode);
                }
                methodNode.AppendChild(paramsNode);
                methodsNode.AppendChild(methodNode);
            }
            return methodsNode;
        }
        #endregion
    }
}
