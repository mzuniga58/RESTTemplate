﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using WizardInstaller.Template.Services;

namespace WizardInstaller.Template.Models
{
    public class ResourceClass
    {
        public string ClassName
        {
            get { return Resource.Name; }
        }

        public CodeElement2 Resource { get; set; }
        public EntityClass Entity { get; set; }

        public ProjectItem ProjectItem
        {
            get
            {
                return Resource.ProjectItem;
            }
        }

        public DBColumn[] Columns
        {
            get
            {
                var codeService = ServiceFactory.GetService<ICodeService>();

                if (Resource.Kind == vsCMElement.vsCMElementClass)
                    return codeService.LoadResourceColumns(this);
                else
                    return codeService.LoadResourceEnumColumns(this);
            }
        }

        public string Namespace
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (Resource.Kind == vsCMElement.vsCMElementClass)
                    return ((CodeClass2)Resource).Namespace.Name;
                else
                    return ((CodeEnum)Resource).Namespace.Name;
            }
        }

        public ResourceType ResourceType
        {
            get
            {
                if (Resource.Kind == vsCMElement.vsCMElementClass)
                    return ResourceType.Class;
                else
                    return ResourceType.Enum;
            }
        }

        public ResourceClass(CodeElement2 code, EntityClass entity)
        {
            Resource = code;
            Entity = entity;
        }

        public ResourceClass()
        {
            Resource = null;
            Entity = null;
        }


        public override string ToString()
        {
            return Resource.Name;
        }
    }
}
